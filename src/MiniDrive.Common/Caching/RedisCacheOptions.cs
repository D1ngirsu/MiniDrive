namespace MiniDrive.Common.Caching;

/// <summary>
/// Configuration settings for Redis caching.
/// </summary>
public sealed class RedisCacheOptions
{
    public const string DefaultSectionName = "Redis";

    /// <summary>
    /// Redis connection string, e.g. "localhost:6379".
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Optional prefix applied to all cache keys to avoid collisions.
    /// </summary>
    public string? KeyPrefix { get; set; } = "minidrive:";

    /// <summary>
    /// Default TTL for cache entries. Null means no expiration.
    /// </summary>
    public TimeSpan? DefaultTtl { get; set; } = TimeSpan.FromMinutes(30);
}

