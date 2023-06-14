using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.DataProtectionLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataProtector(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var dataProtectionProvider = provider.GetRequiredService<IDataProtectionProvider>();

            var dataProtector = dataProtectionProvider.CreateProtector("default");
            return dataProtector;
        });

        return services;
    }

    public static IServiceCollection AddTimeLimitedDataProtector(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var dataProtectionProvider = provider.GetRequiredService<IDataProtectionProvider>();
            var dataProtector = dataProtectionProvider.CreateProtector("default");

            var timeLimitedDataProtector = dataProtector.ToTimeLimitedDataProtector();
            return timeLimitedDataProtector;
        });
        return services;
    }
}