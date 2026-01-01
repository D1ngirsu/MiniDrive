using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MiniDrive.Common.Caching;

public static class RedisCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers Redis-based caching using configuration (e.g. appsettings.json section "Redis").
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = RedisCacheOptions.DefaultSectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<RedisCacheOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            EnsureConnectionString(options);
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>
    /// Registers Redis-based caching using a programmatic configuration action.
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        Action<RedisCacheOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
            EnsureConnectionString(options);
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    private static void EnsureConnectionString(RedisCacheOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }
    }
}

