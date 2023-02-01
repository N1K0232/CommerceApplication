using CommerceApi.Client.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceApi.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailClientSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(EmailClientSettings));
        var settings = section.Get<EmailClientSettings>();

        services.AddSingleton(settings);
        return services;
    }
}