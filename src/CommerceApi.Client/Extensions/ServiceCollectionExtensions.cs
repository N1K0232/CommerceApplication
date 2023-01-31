using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailClientSettings(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}