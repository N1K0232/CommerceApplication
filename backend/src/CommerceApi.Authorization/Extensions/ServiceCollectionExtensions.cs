using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.Authorization.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationHandler<THandler>(this IServiceCollection services, ServiceLifetime handlerLifetime = ServiceLifetime.Scoped) where THandler : class, IAuthorizationHandler
    {
        var authorizationHandlerService = typeof(IAuthorizationHandler);
        var authorizationHandlerImplementation = typeof(THandler);

        var serviceDescriptor = new ServiceDescriptor(authorizationHandlerService, authorizationHandlerImplementation, handlerLifetime);
        services.Add(serviceDescriptor);

        return services;
    }
}