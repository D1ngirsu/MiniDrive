using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Identity;
using MiniDrive.Identity.DTOs;
using MiniDrive.Identity.Entities;
using MiniDrive.Identity.Repositories;
using MiniDrive.Identity.Services;
using Xunit;

namespace MiniDrive.Identity.Tests;

public class AuthServicesTests : IDisposable
{
    private readonly IdentityDbContext _identityContext;
    private readonly UserRepository _userRepository;
    private readonly AuthServices _authServices;

    public AuthServicesTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _identityContext = new IdentityDbContext(options);
        _userRepository = new UserRepository(_identityContext);
        _authServices = new AuthServices(_userRepository);
    }

    public void Dispose()
    {
        _identityContext.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_EmptyEmail_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "",
            Password = "password123"
        };

        // Act
        var result = await _authServices.RegisterAsync(request);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    [Fact]
    public async Task RegisterAsync_EmptyPassword_ReturnsFailure()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act
        var result = await _authServices.RegisterAsync(request);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        _identityContext.Users.Add(existingUser);
        await _identityContext.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = await _authServices.RegisterAsync(request);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("already registered");
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            DisplayName = "Test User"
        };

        // Act
        var result = await _authServices.RegisterAsync(request);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.Session.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
        var saved = await _identityContext.Users.FindAsync(result.User.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        // Act
        var result = await _authServices.LoginAsync(request);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange - Create a user with a valid password first
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "correctpassword123",
            DisplayName = "Test"
        };
        var registerResult = await _authServices.RegisterAsync(registerRequest);
        registerResult.Succeeded.Should().BeTrue();

        // Now try to login with wrong password
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _authServices.LoginAsync(request);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = new Session
        {
            Token = "test-token",
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        };
        _identityContext.Sessions.Add(session);
        await _identityContext.SaveChangesAsync();

        // Act
        var result = await _authServices.LogoutAsync("test-token");

        // Assert
        result.Should().BeTrue();
        var deleted = await _identityContext.Sessions.FindAsync("test-token");
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ValidateSessionAsync_ValidToken_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            DisplayName = "Test",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true
        };
        var session = new Session
        {
            Token = "test-token",
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(12)
        };
        _identityContext.Users.Add(user);
        _identityContext.Sessions.Add(session);
        await _identityContext.SaveChangesAsync();

        // Act
        var result = await _authServices.ValidateSessionAsync("test-token");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }
}

