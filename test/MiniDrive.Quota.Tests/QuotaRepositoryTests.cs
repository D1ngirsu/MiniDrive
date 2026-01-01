using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Quota;
using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Repositories;
using Xunit;

namespace MiniDrive.Quota.Tests;

public class QuotaRepositoryTests : IDisposable
{
    private readonly QuotaDbContext _context;
    private readonly QuotaRepository _repository;

    public QuotaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<QuotaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new QuotaDbContext(options);
        _repository = new QuotaRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetOrCreateAsync_NonExistentQuota_CreatesQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var defaultLimit = 5_000_000_000L;

        // Act
        var result = await _repository.GetOrCreateAsync(userId, defaultLimit);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.LimitBytes.Should().Be(defaultLimit);
        result.UsedBytes.Should().Be(0);
        var saved = await _context.UserQuotas.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrCreateAsync_ExistingQuota_ReturnsQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingQuota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 1_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _context.UserQuotas.Add(existingQuota);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrCreateAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingQuota.Id);
        result.UsedBytes.Should().Be(1_000_000_000);
    }

    [Fact]
    public async Task IncreaseUsedBytesAsync_ValidBytes_IncreasesUsedBytes()
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
        _context.UserQuotas.Add(quota);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IncreaseUsedBytesAsync(userId, 500_000_000);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.UserQuotas.FindAsync(quota.Id);
        updated!.UsedBytes.Should().Be(1_500_000_000);
    }

    [Fact]
    public async Task DecreaseUsedBytesAsync_ValidBytes_DecreasesUsedBytes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 2_000_000_000,
            LimitBytes = 5_000_000_000
        };
        _context.UserQuotas.Add(quota);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DecreaseUsedBytesAsync(userId, 500_000_000);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.UserQuotas.FindAsync(quota.Id);
        updated!.UsedBytes.Should().Be(1_500_000_000);
    }

    [Fact]
    public async Task DecreaseUsedBytesAsync_BelowZero_SetsToZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = new UserQuota
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UsedBytes = 100,
            LimitBytes = 5_000_000_000
        };
        _context.UserQuotas.Add(quota);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DecreaseUsedBytesAsync(userId, 500);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.UserQuotas.FindAsync(quota.Id);
        updated!.UsedBytes.Should().Be(0);
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
        _context.UserQuotas.Add(quota);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateLimitAsync(userId, 10_000_000_000);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.UserQuotas.FindAsync(quota.Id);
        updated!.LimitBytes.Should().Be(10_000_000_000);
    }
}

