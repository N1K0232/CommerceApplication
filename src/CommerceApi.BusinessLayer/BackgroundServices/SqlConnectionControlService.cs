using CommerceApi.DataAccessLayer.Handlers.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommerceApi.BusinessLayer.BackgroundServices;

public class SqlConnectionControlService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SqlConnectionControlService> _logger;

    private PeriodicTimer _timer;

    public SqlConnectionControlService(IServiceProvider serviceProvider, ILogger<SqlConnectionControlService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("testing connection");

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IDbConnectionHandler>();

        _timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await handler.OpenAsync();
            await handler.CloseAsync();
        }
    }

    public override void Dispose()
    {
        _timer.Dispose();
        _timer = null;

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}