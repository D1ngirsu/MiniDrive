using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniDrive.Audit;
using MiniDrive.Common.Caching;
using MiniDrive.Common.Jwt;
using MiniDrive.Files;
using MiniDrive.Folders;
using MiniDrive.Identity;
using MiniDrive.Quota;
using MiniDrive.Storage;

namespace MiniDrive.Api.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Generate unique test ID for this factory instance
            var testId = Guid.NewGuid().ToString("N");

            // Remove all existing DbContext registrations using RemoveAll
            // This removes the DbContext, DbContextOptions, and related factory registrations
            services.RemoveAll(typeof(IdentityDbContext));
            services.RemoveAll(typeof(FileDbContext));
            services.RemoveAll(typeof(FolderDbContext));
            services.RemoveAll(typeof(AuditDbContext));
            services.RemoveAll(typeof(QuotaDbContext));

            // Remove DbContextOptions registrations explicitly
            services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
            services.RemoveAll(typeof(DbContextOptions<FileDbContext>));
            services.RemoveAll(typeof(DbContextOptions<FolderDbContext>));
            services.RemoveAll(typeof(DbContextOptions<AuditDbContext>));
            services.RemoveAll(typeof(DbContextOptions<QuotaDbContext>));

            // Re-register with InMemory databases
            // The key is that we're completely replacing the registration, not adding to it
            services.AddDbContext<IdentityDbContext>(options =>
            {
                // Clear any existing extensions and use InMemory
                options.UseInMemoryDatabase($"IdentityDb_Test_{testId}");
            }, ServiceLifetime.Scoped);

            services.AddDbContext<FileDbContext>(options =>
            {
                options.UseInMemoryDatabase($"FileDb_Test_{testId}");
            }, ServiceLifetime.Scoped);

            services.AddDbContext<FolderDbContext>(options =>
            {
                options.UseInMemoryDatabase($"FolderDb_Test_{testId}");
            }, ServiceLifetime.Scoped);

            services.AddDbContext<AuditDbContext>(options =>
            {
                options.UseInMemoryDatabase($"AuditDb_Test_{testId}");
            }, ServiceLifetime.Scoped);

            services.AddDbContext<QuotaDbContext>(options =>
            {
                options.UseInMemoryDatabase($"QuotaDb_Test_{testId}");
            }, ServiceLifetime.Scoped);

            // Replace cache with in-memory implementation
            services.RemoveAll(typeof(ICacheService));
            services.AddSingleton<ICacheService, InMemoryCacheService>();

            // Configure storage to use a temporary directory
            services.Configure<StorageOptions>(options =>
            {
                options.BasePath = Path.Combine(Path.GetTempPath(), "MiniDrive_Test_Storage");
                options.MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB
            });

            // Configure JWT with test values
            services.Configure<JwtOptions>(options =>
            {
                options.SigningKey = "TestSecretKeyForIntegrationTests12345678901234567890";
                options.Issuer = "MiniDrive.Test";
                options.Audience = "MiniDrive.Test";
                options.AccessTokenLifetime = TimeSpan.FromMinutes(60);
            });
        });

        builder.UseEnvironment("Testing");
    }
}

// Simple in-memory cache implementation for testing
public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime? Expiry)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.Expiry == null || item.Expiry > DateTime.UtcNow)
            {
                return Task.FromResult((T?)item.Value);
            }
            _cache.Remove(key);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var expiry = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        _cache[key] = (value!, expiry);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.Remove(key));
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var item))
        {
            if (item.Expiry == null || item.Expiry > DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }
            _cache.Remove(key);
        }
        return Task.FromResult(false);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, ttl, cancellationToken);
        return value;
    }
}

