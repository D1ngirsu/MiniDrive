using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Services;
using MiniDrive.Clients.Quota;

namespace MiniDrive.Files.Api.Adapters;

/// <summary>
/// Adapter that implements IQuotaService using HTTP client calls to Quota microservice.
/// </summary>
public class QuotaServiceAdapter : IQuotaService
{
    private readonly IQuotaClient _quotaClient;

    public QuotaServiceAdapter(IQuotaClient quotaClient)
    {
        _quotaClient = quotaClient;
    }

    public async Task<bool> CanUploadAsync(Guid userId, long fileSize)
    {
        return await _quotaClient.CanUploadAsync(userId, fileSize);
    }

    public async Task<bool> IncreaseAsync(Guid userId, long bytes)
    {
        return await _quotaClient.IncreaseAsync(userId, bytes);
    }

    public async Task<bool> DecreaseAsync(Guid userId, long bytes)
    {
        return await _quotaClient.DecreaseAsync(userId, bytes);
    }

    public async Task<UserQuota?> GetQuotaAsync(Guid userId)
    {
        var quotaInfo = await _quotaClient.GetQuotaAsync(userId);
        if (quotaInfo == null)
        {
            return null;
        }

        return new UserQuota
        {
            Id = Guid.NewGuid(), // Not available from API
            UserId = quotaInfo.UserId,
            UsedBytes = quotaInfo.UsedBytes,
            LimitBytes = quotaInfo.LimitBytes
        };
    }

    public async Task<UserQuota> GetOrCreateQuotaAsync(Guid userId, long defaultLimitBytes = 5L * 1024 * 1024 * 1024)
    {
        var quota = await GetQuotaAsync(userId);
        if (quota != null)
        {
            return quota;
        }

        // If quota doesn't exist, create it with default limit
        // Note: This would require an API endpoint to create quota
        // For now, return a default quota
        return new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 0,
            LimitBytes = defaultLimitBytes
        };
    }

    public Task<bool> UpdateLimitAsync(Guid userId, long limitBytes)
    {
        // Not implemented in client - would need API endpoint
        return Task.FromResult(false);
    }

    public Task<bool> SyncUsedBytesAsync(Guid userId, long actualUsedBytes)
    {
        // Not implemented in client - would need API endpoint
        return Task.FromResult(false);
    }
}

