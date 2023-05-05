using CommerceApi.DataAccessLayer.Abstractions;
using CommerceApi.DataAccessLayer.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.DataAccessLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlContext(this IServiceCollection services, Action<SqlContextOptions> configuration)
    {
        var options = new SqlContextOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<ISqlContext, SqlContext>();

        return services;
    }
}