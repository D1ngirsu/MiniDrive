using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Identity;
using MiniDrive.Identity.Entities;
using MiniDrive.Identity.Repositories;
using Xunit;

namespace MiniDrive.Identity.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new IdentityDbContext(options);
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ValidUser_CreatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        var saved = await _context.Users.FindAsync(user.Id);
        saved.Should().NotBeNull();
        saved!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task CreateSessionAsync_ValidRequest_CreatesSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lifetime = TimeSpan.FromHours(12);

        // Act
        var result = await _repository.CreateSessionAsync(userId, lifetime, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(userId);
        var saved = await _context.Sessions.FindAsync(result.Token);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSessionAsync_ExpiredSession_ReturnsNull()
    {
        // Arrange
        var session = new Session
        {
            Token = "expired-token",
            UserId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow.AddHours(-13),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(-1)
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionAsync("expired-token");

        // Assert
        result.Should().BeNull();
        var deleted = await _context.Sessions.FindAsync("expired-token");
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveSessionAsync_ExistingSession_ReturnsTrue()
    {
        // Arrange
        var session = new Session
        {
            Token = "test-token",
            UserId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RemoveSessionAsync("test-token");

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Sessions.FindAsync("test-token");
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveSessionsForUserAsync_RemovesAllSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = new Session
        {
            Token = "token1",
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        };
        var session2 = new Session
        {
            Token = "token2",
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        };
        _context.Sessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RemoveSessionsForUserAsync(userId);

        // Assert
        result.Should().Be(2);
        var deleted1 = await _context.Sessions.FindAsync("token1");
        var deleted2 = await _context.Sessions.FindAsync("token2");
        deleted1.Should().BeNull();
        deleted2.Should().BeNull();
    }
}

