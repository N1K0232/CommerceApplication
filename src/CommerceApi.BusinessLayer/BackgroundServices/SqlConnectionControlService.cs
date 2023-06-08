using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommerceApi.BusinessLayer.BackgroundServices;

public class SqlConnectionControlService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlConnectionControlService> _logger;

    public SqlConnectionControlService(IConfiguration configuration, ILogger<SqlConnectionControlService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("testing connection");

        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sqlConnectionString = _configuration.GetConnectionString("SqlConnection");
                using var sqlConnection = new SqlConnection(sqlConnectionString);

                await sqlConnection.OpenAsync(stoppingToken);
                await sqlConnection.CloseAsync();

                _logger.LogInformation("connection test succeeded");
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "connection test failed");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "connection test failed");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "the operation was canceled");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "the operation was canceled");
            }
        }
    }
}