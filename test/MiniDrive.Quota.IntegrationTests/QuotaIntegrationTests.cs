using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Quota;
using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Repositories;
using MiniDrive.Quota.Services;
using Xunit;

namespace MiniDrive.Quota.IntegrationTests;

public class QuotaIntegrationTests : IDisposable
{
    private readonly QuotaDbContext _context;
    private readonly QuotaRepository _quotaRepository;
    private readonly QuotaService _quotaService;
    private readonly string _dbFile;

    public QuotaIntegrationTests()
    {
        _dbFile = $"{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<QuotaDbContext>()
            .UseSqlite($"Data Source={_dbFile}")
            .Options;

        _context = new QuotaDbContext(options);
        _context.Database.EnsureCreated();

        _quotaRepository = new QuotaRepository(_context);
        _quotaService = new QuotaService(_quotaRepository);
    }

    [Fact]
    public async Task CanUploadAsync_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileSize = 1024 * 1024; // 1MB

        // Act
        var canUpload = await _quotaService.CanUploadAsync(userId, fileSize);

        // Assert
        canUpload.Should().BeTrue();
    }

    [Fact]
    public async Task CanUploadAsync_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota = await _quotaRepository.GetOrCreateAsync(userId);
        var fileSize = quota.LimitBytes + 1; // Exceeds limit

        // Act
        var canUpload = await _quotaService.CanUploadAsync(userId, fileSize);

        // Assert
        canUpload.Should().BeFalse();
    }

    [Fact]
    public async Task IncreaseAsync_ValidBytes_IncreasesUsedBytes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var initialQuota = await _quotaRepository.GetOrCreateAsync(userId);
        var initialUsed = initialQuota.UsedBytes;
        var bytesToAdd = 1024 * 1024; // 1MB

        // Act
        var result = await _quotaService.IncreaseAsync(userId, bytesToAdd);

        // Assert
        result.Should().BeTrue();
        var updatedQuota = await _quotaRepository.GetOrCreateAsync(userId);
        updatedQuota.UsedBytes.Should().Be(initialUsed + bytesToAdd);
    }

    [Fact]
    public async Task DecreaseAsync_ValidBytes_DecreasesUsedBytes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Create quota first
        await _quotaRepository.GetOrCreateAsync(userId);
        var bytesToAdd = 2 * 1024 * 1024; // 2MB
        await _quotaService.IncreaseAsync(userId, bytesToAdd);
        var initialQuota = await _quotaRepository.GetOrCreateAsync(userId);
        var initialUsed = initialQuota.UsedBytes;

        // Act
        var result = await _quotaService.DecreaseAsync(userId, bytesToAdd / 2);

        // Assert
        result.Should().BeTrue();
        var updatedQuota = await _quotaRepository.GetOrCreateAsync(userId);
        updatedQuota.UsedBytes.Should().Be(initialUsed - bytesToAdd / 2);
    }

    [Fact]
    public async Task DecreaseAsync_BelowZero_ClampsToZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Create quota first
        await _quotaRepository.GetOrCreateAsync(userId);
        var bytesToAdd = 1024 * 1024; // 1MB
        await _quotaService.IncreaseAsync(userId, bytesToAdd);

        // Act
        var result = await _quotaService.DecreaseAsync(userId, bytesToAdd * 2); // Try to decrease more than used

        // Assert
        result.Should().BeTrue();
        var quota = await _quotaRepository.GetOrCreateAsync(userId);
        quota.UsedBytes.Should().Be(0);
    }

    [Fact]
    public async Task GetQuotaAsync_NewUser_CreatesDefaultQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var quota = await _quotaRepository.GetOrCreateAsync(userId);

        // Assert
        quota.Should().NotBeNull();
        quota.UserId.Should().Be(userId);
        quota.UsedBytes.Should().Be(0);
        quota.LimitBytes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetQuotaAsync_ExistingUser_ReturnsExistingQuota()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var quota1 = await _quotaRepository.GetOrCreateAsync(userId);

        // Act
        var quota2 = await _quotaRepository.GetOrCreateAsync(userId);

        // Assert
        quota2.Should().NotBeNull();
        quota2.Id.Should().Be(quota1.Id);
    }

    [Fact]
    public async Task GetQuotaAsync_WithUsage_ReturnsQuotaWithTotal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Create quota first
        await _quotaRepository.GetOrCreateAsync(userId);
        await _quotaService.IncreaseAsync(userId, 1024 * 1024); // 1MB
        await _quotaService.IncreaseAsync(userId, 512 * 1024); // 512KB

        // Act
        var quota = await _quotaService.GetQuotaAsync(userId);

        // Assert
        quota.Should().NotBeNull();
        quota!.UsedBytes.Should().Be(1536 * 1024); // 1.5MB
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        // Clean up SQLite file
        if (File.Exists(_dbFile))
        {
            File.Delete(_dbFile);
        }
    }
}

