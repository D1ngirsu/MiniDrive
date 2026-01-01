using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Audit;
using MiniDrive.Audit.Entities;
using MiniDrive.Audit.Repositories;
using MiniDrive.Audit.Services;
using Xunit;

namespace MiniDrive.Audit.IntegrationTests;

public class AuditIntegrationTests : IDisposable
{
    private readonly AuditDbContext _context;
    private readonly AuditRepository _auditRepository;
    private readonly AuditService _auditService;
    private readonly string _dbFile;

    public AuditIntegrationTests()
    {
        _dbFile = $"{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlite($"Data Source={_dbFile}")
            .Options;

        _context = new AuditDbContext(options);
        _context.Database.EnsureCreated();

        _auditRepository = new AuditRepository(_context);
        _auditService = new AuditService(_auditRepository);
    }

    [Fact]
    public async Task LogAsync_ValidLogEntry_CreatesAuditLog()
    {
        // Arrange
        var logEntry = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "TestAction",
            EntityType = "TestEntity",
            EntityId = Guid.NewGuid().ToString(),
            IsSuccess = true
        };

        // Act
        await _auditService.LogAsync(logEntry);

        // Assert
        var logs = await _auditRepository.GetByEntityAsync("TestEntity", logEntry.EntityId);
        var savedLog = logs.FirstOrDefault(l => l.Id == logEntry.Id);
        savedLog.Should().NotBeNull();
        savedLog!.Action.Should().Be("TestAction");
        savedLog.EntityType.Should().Be("TestEntity");
    }

    [Fact]
    public async Task LogActionAsync_ValidAction_CreatesAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid().ToString();

        // Act
        await _auditService.LogActionAsync(
            userId,
            "FileUpload",
            "File",
            entityId,
            isSuccess: true,
            details: "File uploaded successfully",
            ipAddress: "127.0.0.1",
            userAgent: "TestAgent");

        // Assert
        var logs = await _auditRepository.GetByUserIdAsync(userId);
        logs.Should().NotBeNull();
        logs.Should().Contain(log => log.Action == "FileUpload" && log.EntityId == entityId);
    }

    [Fact]
    public async Task LogActionAsync_WithFailure_CreatesFailureLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid().ToString();

        // Act
        await _auditService.LogActionAsync(
            userId,
            "FileUpload",
            "File",
            entityId,
            isSuccess: false,
            errorMessage: "File too large",
            ipAddress: "127.0.0.1",
            userAgent: "TestAgent");

        // Assert
        var logs = await _auditRepository.GetByUserIdAsync(userId);
        logs.Should().NotBeNull();
        var log = logs.FirstOrDefault(l => l.EntityId == entityId);
        log.Should().NotBeNull();
        log!.IsSuccess.Should().BeFalse();
        log.ErrorMessage.Should().Be("File too large");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithLogs_ReturnsUserLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _auditService.LogActionAsync(userId, "Action1", "Entity", "1");
        await _auditService.LogActionAsync(userId, "Action2", "Entity", "2");

        // Act
        var logs = await _auditRepository.GetByUserIdAsync(userId);

        // Assert
        logs.Should().NotBeNull();
        logs.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetByEntityAsync_WithLogs_ReturnsEntityLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityId = "TestEntity123";
        await _auditService.LogActionAsync(userId, "Action1", "Entity", entityId);
        await _auditService.LogActionAsync(userId, "Action2", "Entity", entityId);

        // Act
        var logs = await _auditRepository.GetByEntityAsync("Entity", entityId);

        // Assert
        logs.Should().NotBeNull();
        logs.Count().Should().BeGreaterThanOrEqualTo(2);
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

