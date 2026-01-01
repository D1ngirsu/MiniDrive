using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Audit;
using MiniDrive.Audit.Entities;
using MiniDrive.Audit.Repositories;
using MiniDrive.Audit.Services;
using Xunit;

namespace MiniDrive.Audit.Tests;

public class AuditServiceTests : IDisposable
{
    private readonly AuditDbContext _auditContext;
    private readonly AuditRepository _auditRepository;
    private readonly AuditService _auditService;

    public AuditServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _auditContext = new AuditDbContext(options);
        _auditRepository = new AuditRepository(_auditContext);
        _auditService = new AuditService(_auditRepository);
    }

    public void Dispose()
    {
        _auditContext.Dispose();
    }

    [Fact]
    public async Task LogAsync_NullEntry_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _auditService.LogAsync(null!));
    }

    [Fact]
    public async Task LogAsync_ValidEntry_CreatesLog()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "TestAction",
            EntityType = "TestEntity",
            EntityId = Guid.NewGuid().ToString()
        };

        // Act
        await _auditService.LogAsync(auditLog);

        // Assert
        var saved = await _auditContext.AuditLogs.FindAsync(auditLog.Id);
        saved.Should().NotBeNull();
        saved!.Action.Should().Be("TestAction");
    }

    [Fact]
    public async Task LogActionAsync_EmptyAction_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _auditService.LogActionAsync(
            Guid.NewGuid(), "", "EntityType", "EntityId"));
    }

    [Fact]
    public async Task LogActionAsync_EmptyEntityType_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _auditService.LogActionAsync(
            Guid.NewGuid(), "Action", "", "EntityId"));
    }

    [Fact]
    public async Task LogActionAsync_EmptyEntityId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _auditService.LogActionAsync(
            Guid.NewGuid(), "Action", "EntityType", ""));
    }

    [Fact]
    public async Task LogActionAsync_ValidParameters_CreatesLog()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _auditService.LogActionAsync(
            userId, "TestAction", "TestEntity", "EntityId", true, "Details", null, "127.0.0.1", "UserAgent");

        // Assert
        var logs = await _auditContext.AuditLogs
            .Where(log => log.UserId == userId && log.Action == "TestAction")
            .ToListAsync();
        logs.Should().HaveCount(1);
        var log = logs.First();
        log.EntityType.Should().Be("TestEntity");
        log.EntityId.Should().Be("EntityId");
        log.IsSuccess.Should().BeTrue();
        log.Details.Should().Be("Details");
        log.IpAddress.Should().Be("127.0.0.1");
        log.UserAgent.Should().Be("UserAgent");
    }
}

