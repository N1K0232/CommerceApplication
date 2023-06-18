using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.TenantContext;

public static class TenantContextServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContextAccessor(this IServiceCollection services, Action<TenantContextOptions> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var options = new TenantContextOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();
        return services;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.ApplicationServices.GetService(typeof(ITenantContextAccessor)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services.");
        }

        return app.UseMiddleware<TenantContextMiddleware>();
    }
}