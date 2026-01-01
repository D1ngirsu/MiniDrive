using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Identity;
using MiniDrive.Identity.DTOs;
using MiniDrive.Identity.Entities;
using MiniDrive.Identity.Repositories;
using MiniDrive.Identity.Services;
using Xunit;

namespace MiniDrive.Identity.IntegrationTests;

public class IdentityIntegrationTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly AuthServices _authServices;
    private readonly string _dbFile;

    public IdentityIntegrationTests()
    {
        _dbFile = $"{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite($"Data Source={_dbFile}")
            .Options;

        _context = new IdentityDbContext(options);
        _context.Database.EnsureCreated();

        _userRepository = new UserRepository(_context);
        _authServices = new AuthServices(_userRepository);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_CreatesUserAndSession()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User"
        };

        // Act
        var result = await _authServices.RegisterAsync(request, "TestAgent", "127.0.0.1");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
        result.Session.Should().NotBeNull();
        result.Session!.Token.Should().NotBeNullOrEmpty();

        // Verify user is persisted
        var user = await _userRepository.GetByEmailAsync("test@example.com");
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var request1 = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "Password123!"
        };
        await _authServices.RegisterAsync(request1);

        var request2 = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authServices.RegisterAsync(request2);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("already registered");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSession()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "login@example.com",
            Password = "Password123!"
        };
        await _authServices.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authServices.LoginAsync(loginRequest, "TestAgent", "127.0.0.1");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.Session.Should().NotBeNull();
        result.User!.LastLoginAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "login2@example.com",
            Password = "Password123!"
        };
        await _authServices.RegisterAsync(registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login2@example.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await _authServices.LoginAsync(loginRequest);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task ValidateSessionAsync_ValidToken_ReturnsUser()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "validate@example.com",
            Password = "Password123!"
        };
        var registerResult = await _authServices.RegisterAsync(registerRequest);
        var token = registerResult.Session!.Token;

        // Act
        var user = await _authServices.ValidateSessionAsync(token);

        // Assert
        user.Should().NotBeNull();
        user!.Email.Should().Be("validate@example.com");
    }

    [Fact]
    public async Task ValidateSessionAsync_InvalidToken_ReturnsNull()
    {
        // Act
        var user = await _authServices.ValidateSessionAsync("invalid-token");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RemovesSession()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "logout@example.com",
            Password = "Password123!"
        };
        var registerResult = await _authServices.RegisterAsync(registerRequest);
        var token = registerResult.Session!.Token;

        // Act
        var removed = await _authServices.LogoutAsync(token);

        // Assert
        removed.Should().BeTrue();

        // Verify session is invalid
        var user = await _authServices.ValidateSessionAsync(token);
        user.Should().BeNull();
    }

    [Fact]
    public async Task LogoutAllAsync_RemovesAllUserSessions()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "logoutall@example.com",
            Password = "Password123!"
        };
        var registerResult = await _authServices.RegisterAsync(registerRequest);
        var userId = registerResult.User!.Id;
        
        // Create second session by logging in again
        var loginRequest = new LoginRequest
        {
            Email = "logoutall@example.com",
            Password = "Password123!"
        };
        var loginResult = await _authServices.LoginAsync(loginRequest);
        
        var token1 = registerResult.Session!.Token;
        var token2 = loginResult.Session!.Token;

        // Act
        var count = await _authServices.LogoutAllAsync(userId);

        // Assert
        count.Should().BeGreaterThan(0);

        // Verify all sessions are invalid
        var user1 = await _authServices.ValidateSessionAsync(token1);
        var user2 = await _authServices.ValidateSessionAsync(token2);
        user1.Should().BeNull();
        user2.Should().BeNull();
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

