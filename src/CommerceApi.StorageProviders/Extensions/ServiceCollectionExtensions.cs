using CommerceApi.StorageProviders.Abstractions;
using CommerceApi.StorageProviders.Azure;
using CommerceApi.StorageProviders.FileSystem;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSystemStorageProvider(this IServiceCollection services, Action<FileSystemSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var fileSystemSettings = new FileSystemSettings();
        configuration.Invoke(fileSystemSettings);

        services.AddSingleton(fileSystemSettings);
        services.AddStorageProvider<FileSystemStorageProvider>();

        return services;
    }

    public static IServiceCollection AddAzureStorageProvider(this IServiceCollection services, Action<AzureStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var azureStorageSettings = new AzureStorageSettings();
        configuration.Invoke(azureStorageSettings);

        services.AddSingleton(azureStorageSettings);
        services.AddStorageProvider<AzureStorageProvider>();

        return services;
    }

    public static IServiceCollection AddAzureStorageProvider(this IServiceCollection services, Action<IServiceProvider, AzureStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services.AddScoped(provider =>
        {
            var azureStorageSettings = new AzureStorageSettings();
            configuration.Invoke(provider, azureStorageSettings);

            return azureStorageSettings;
        });

        services.AddStorageProvider<AzureStorageProvider>();
        return services;
    }

    private static IServiceCollection AddStorageProvider<TStorage>(this IServiceCollection services) where TStorage : StorageProvider
    {
        services.AddScoped<IStorageProvider, TStorage>();
        return services;
    }
}