using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Quota;
using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Repositories;
using MiniDrive.Quota.Services;
using Xunit;

namespace MiniDrive.Quota.Tests;

public class QuotaServiceTests : IDisposable
{
    private readonly QuotaDbContext _quotaContext;
    private readonly QuotaRepository _quotaRepository;
    private readonly QuotaService _quotaService;

    public QuotaServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuotaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _quotaContext = new QuotaDbContext(options);
        _quotaRepository = new QuotaRepository(_quotaContext);
        _quotaService = new QuotaService(_quotaRepository);
    }

    public void Dispose()
    {
        _quotaContext.Dispose();
    }

    [Fact]
    public async Task CanUploadAsync_NegativeFileSize_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _quotaService.CanUploadAsync(userId, -1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadAsync_QuotaExceeded_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 4_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.CanUploadAsync(userId, 2_000_000_000);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadAsync_QuotaAvailable_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 1_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.CanUploadAsync(userId, 1_000_000_000);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IncreaseAsync_NegativeBytes_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _quotaService.IncreaseAsync(userId, -1));
    }

    [Fact]
    public async Task IncreaseAsync_ValidBytes_IncreasesQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 0,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.IncreaseAsync(userId, 1024);

        // Assert
        result.Should().BeTrue();
        var updated = await _quotaContext.UserQuotas.FindAsync(quota.Id);
        updated!.UsedBytes.Should().Be(1024);
    }

    [Fact]
    public async Task DecreaseAsync_NegativeBytes_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _quotaService.DecreaseAsync(userId, -1));
    }

    [Fact]
    public async Task DecreaseAsync_ValidBytes_DecreasesQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 2048,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.DecreaseAsync(userId, 1024);

        // Assert
        result.Should().BeTrue();
        var updated = await _quotaContext.UserQuotas.FindAsync(quota.Id);
        updated!.UsedBytes.Should().Be(1024);
    }

    [Fact]
    public async Task GetQuotaAsync_ExistingQuota_ReturnsQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 1_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.GetQuotaAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.UsedBytes.Should().Be(1_000_000_000);
    }

    [Fact]
    public async Task UpdateLimitAsync_NegativeLimit_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _quotaService.UpdateLimitAsync(userId, -1));
    }

    [Fact]
    public async Task UpdateLimitAsync_ValidLimit_UpdatesLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 1_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _quotaContext.UserQuotas.Add(quota);
        await _quotaContext.SaveChangesAsync();

        // Act
        var result = await _quotaService.UpdateLimitAsync(userId, 10_000_000_000);

        // Assert
        result.Should().BeTrue();
        var updated = await _quotaContext.UserQuotas.FindAsync(quota.Id);
        updated!.LimitBytes.Should().Be(10_000_000_000);
    }
}

