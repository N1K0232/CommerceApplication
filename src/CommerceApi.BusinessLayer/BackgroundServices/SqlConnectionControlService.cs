using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommerceApi.BusinessLayer.BackgroundServices;

public class SqlConnectionControlService : BackgroundService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<SqlConnectionControlService> logger;

    public SqlConnectionControlService(IConfiguration configuration, ILogger<SqlConnectionControlService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("testing connection");

        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (!await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var sqlConnectionString = configuration.GetConnectionString("SqlConnection");
                using var sqlConnection = new SqlConnection(sqlConnectionString);

                await sqlConnection.OpenAsync(stoppingToken);
                await sqlConnection.CloseAsync();
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "connection test failed");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "connection test failed");
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "the operation was canceled");
            }
            catch (OperationCanceledException ex)
            {
                logger.LogError(ex, "the operation was canceled");
            }
        }
    }
}