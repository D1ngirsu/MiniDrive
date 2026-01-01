using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MiniDrive.Storage;

/// <summary>
/// Extension methods for registering storage services.
/// </summary>
public static class StorageServiceCollectionExtensions
{
    /// <summary>
    /// Adds file storage services to the service collection.
    /// </summary>
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        Action<StorageOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<StorageOptions>(options => { });
        }

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        return services;
    }
}

