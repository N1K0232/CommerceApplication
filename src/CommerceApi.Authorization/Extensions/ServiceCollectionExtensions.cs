using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.Authorization.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationHandler<THandler>(this IServiceCollection services, ServiceLifetime handlerLifetime = ServiceLifetime.Scoped) where THandler : class, IAuthorizationHandler
    {
        var serviceType = typeof(IAuthorizationHandler);
        var implementationType = typeof(THandler);

        var serviceDescriptor = new ServiceDescriptor(serviceType, implementationType, handlerLifetime);
        services.Add(serviceDescriptor);

        return services;
    }
}