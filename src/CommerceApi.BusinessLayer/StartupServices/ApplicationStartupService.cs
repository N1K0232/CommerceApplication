using CommerceApi.DataAccessLayer.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommerceApi.BusinessLayer.StartupServices;

public class ApplicationStartupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;

    public ApplicationStartupService(IServiceProvider serviceProvider, IMemoryCache cache)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var dataContext = services.GetRequiredService<IDataContext>();
        await dataContext.EnsureCreatedAsync();
        await dataContext.MigrateAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cache.Dispose();
        return Task.CompletedTask;
    }
}