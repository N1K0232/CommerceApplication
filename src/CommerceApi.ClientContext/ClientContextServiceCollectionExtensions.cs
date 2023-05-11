using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceApi.ClientContext;

public static class ClientContextServiceCollectionExtensions
{
    public static IServiceCollection AddClientContextAccessor(this IServiceCollection services, Action<ClientContextOptions> configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ClientContextOptions();
        configuration?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<IClientContextAccessor, ClientContextAccessor>();
        return services;
    }

    public static SwaggerGenOptions AddClientContextOperationFilter(this SwaggerGenOptions swaggerGenOptions)
    {
        ArgumentNullException.ThrowIfNull(swaggerGenOptions);

        swaggerGenOptions.OperationFilter<ClientContextOperationFilter>();
        return swaggerGenOptions;
    }

    public static SwaggerGenOptions MapType<T>(this SwaggerGenOptions swaggerGenOptions, string type, string format, string example = null)
    {
        ArgumentNullException.ThrowIfNull(swaggerGenOptions);
        ArgumentNullException.ThrowIfNullOrEmpty(type);
        ArgumentNullException.ThrowIfNullOrEmpty(format);

        swaggerGenOptions.MapType<T>(() => new OpenApiSchema()
        {
            Type = type,
            Format = format,
            Example = !string.IsNullOrWhiteSpace(example) ? new OpenApiString(example) : null
        });

        return swaggerGenOptions;
    }

    public static IApplicationBuilder UseClientContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.ApplicationServices.GetService(typeof(IClientContextAccessor)) is null)
        {
            throw new InvalidOperationException("Unable to find the required services.");
        }

        app.UseMiddleware<ClientContextMiddleware>();
        return app;
    }
}