using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
        });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(t => t.FullName.EndsWith("Service")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(classes => classes.Where(t => t.FullName.EndsWith("Manager")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}