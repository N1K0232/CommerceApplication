using CommerceApi.Security.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommerceApi.Security.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPasswordHasher(this IServiceCollection services)
        {
            services.TryAddScoped<IPasswordHasher, PasswordHasher>();
            return services;
        }

        public static IServiceCollection AddStringHasher(this IServiceCollection services)
        {
            services.TryAddSingleton<IStringHasher, StringHasher>();
            return services;
        }

        public static IServiceCollection AddPathGenerator(this IServiceCollection services)
        {
            services.TryAddSingleton<IPathGenerator, PathGenerator>();
            return services;
        }
    }
}