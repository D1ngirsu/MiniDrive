using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniDrive.Sharing.Repositories;
using MiniDrive.Sharing.Services;

namespace MiniDrive.Sharing;

/// <summary>
/// Extension methods for registering Sharing services in the DI container.
/// </summary>
public static class SharingServiceCollectionExtensions
{
    /// <summary>
    /// Adds sharing services and database context to the DI container.
    /// </summary>
    public static IServiceCollection AddShareServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<SharingDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ShareRepository>();
        services.AddScoped<ShareService>();

        return services;
    }
}
