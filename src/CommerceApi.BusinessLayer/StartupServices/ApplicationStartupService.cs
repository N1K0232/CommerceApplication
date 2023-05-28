using CommerceApi.DataAccessLayer.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommerceApi.BusinessLayer.StartupServices;

public class ApplicationStartupService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IMemoryCache cache;

    public ApplicationStartupService(IServiceProvider serviceProvider, IMemoryCache cache)
    {
        this.serviceProvider = serviceProvider;
        this.cache = cache;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var dataContext = services.GetRequiredService<IDataContext>();
        await dataContext.EnsureCreatedAsync();
        await dataContext.MigrateAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cache.Dispose();
        return Task.CompletedTask;
    }
}