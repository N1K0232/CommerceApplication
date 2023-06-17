using CommerceApi.DataAccessLayer;
using CommerceApi.DataAccessLayer.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommerceApi.BusinessLayer.StartupServices;

public class ApplicationStartupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationStartupService> _logger;
    private readonly IMemoryCache _cache;

    public ApplicationStartupService(IServiceProvider serviceProvider, ILogger<ApplicationStartupService> logger, IMemoryCache cache)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cache = cache;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var dataContext = services.GetRequiredService<ICommerceApplicationDbContext>();
        var dataContextConnectionResult = await dataContext.TestConnectionAsync();
        if (dataContextConnectionResult)
        {
            _logger.LogInformation("connection test succeeded for {dataContext} object", typeof(CommerceApplicationDbContext).Name);

            await dataContext.EnsureCreatedAsync();
            await dataContext.MigrateAsync();
        }
        else
        {
            _logger.LogError("connection test failed for {dataContext} object", typeof(CommerceApplicationDbContext).Name);
        }

        var sqlContext = services.GetRequiredService<ICommerceApplicationSqlContext>();
        var sqlContextConnectionResult = await sqlContext.TestConnectionAsync();
        if (sqlContextConnectionResult)
        {
            _logger.LogInformation("connection test succeeded for {sqlContext} object", typeof(CommerceApplicationSqlContext).Name);
        }
        else
        {
            _logger.LogError("connection test failed for {sqlContext} object", typeof(CommerceApplicationSqlContext).Name);
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cache.Dispose();
        return Task.CompletedTask;
    }
}