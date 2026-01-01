using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Audit;
using MiniDrive.Audit.Entities;
using MiniDrive.Audit.Repositories;
using Xunit;

namespace MiniDrive.Audit.Tests;

public class AuditRepositoryTests : IDisposable
{
    private readonly AuditDbContext _context;
    private readonly AuditRepository _repository;

    public AuditRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AuditDbContext(options);
        _repository = new AuditRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ValidLog_CreatesLog()
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
        var result = await _repository.CreateAsync(auditLog);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(auditLog.Id);
        var saved = await _context.AuditLogs.FindAsync(auditLog.Id);
        saved.Should().NotBeNull();
        saved!.Action.Should().Be("TestAction");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var log1 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = "Action1",
            EntityType = "Entity",
            EntityId = "1"
        };
        var log2 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = "Action2",
            EntityType = "Entity",
            EntityId = "2"
        };
        var log3 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "Action3",
            EntityType = "Entity",
            EntityId = "3"
        };
        _context.AuditLogs.AddRange(log1, log2, log3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(log => log.Id == log1.Id);
        result.Should().Contain(log => log.Id == log2.Id);
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsEntityLogs()
    {
        // Arrange
        var entityType = "File";
        var entityId = "123";
        var log1 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "Create",
            EntityType = entityType,
            EntityId = entityId
        };
        var log2 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "Update",
            EntityType = entityType,
            EntityId = entityId
        };
        _context.AuditLogs.AddRange(log1, log2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEntityAsync(entityType, entityId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByActionAsync_ReturnsActionLogs()
    {
        // Arrange
        var action = "FileUpload";
        var log1 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = action,
            EntityType = "File",
            EntityId = "1"
        };
        var log2 = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = action,
            EntityType = "File",
            EntityId = "2"
        };
        _context.AuditLogs.AddRange(log1, log2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByActionAsync(action);

        // Assert
        result.Should().HaveCount(2);
    }
}

