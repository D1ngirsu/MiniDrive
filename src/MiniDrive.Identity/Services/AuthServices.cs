using System.Security.Cryptography;
using MiniDrive.Identity.DTOs;
using MiniDrive.Identity.Entities;
using MiniDrive.Identity.Repositories;

namespace MiniDrive.Identity.Services;

public class AuthServices
{
    private readonly UserRepository _userRepository;
    private readonly TimeSpan _sessionLifetime;

    public AuthServices(UserRepository userRepository, TimeSpan? sessionLifetime = null)
    {
        _userRepository = userRepository;
        _sessionLifetime = sessionLifetime ?? TimeSpan.FromHours(12);
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        string? userAgent = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult.Failure("Email and password are required.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing is not null)
        {
            return AuthResult.Failure("Email is already registered.");
        }

        var (hash, salt) = HashPassword(request.Password);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? email
                : request.DisplayName.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        var session = await _userRepository.CreateSessionAsync(
            user.Id,
            _sessionLifetime,
            userAgent,
            ipAddress);

        return AuthResult.Success(user, session);
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        string? userAgent = null,
        string? ipAddress = null)
    {
        var email = request.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult.Failure("Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return AuthResult.Failure("Invalid credentials.");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return AuthResult.Failure("Invalid credentials.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var session = await _userRepository.CreateSessionAsync(
            user.Id,
            _sessionLifetime,
            userAgent,
            ipAddress);

        return AuthResult.Success(user, session);
    }

    public Task<bool> LogoutAsync(string token) =>
        _userRepository.RemoveSessionAsync(token);

    public Task<int> LogoutAllAsync(Guid userId) =>
        _userRepository.RemoveSessionsForUserAsync(userId);

    public async Task<User?> ValidateSessionAsync(string token)
    {
        var session = await _userRepository.GetSessionAsync(token);
        if (session is null)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(session.UserId);
        return user is not null && user.IsActive ? user : null;
    }

    public Task CleanupAsync() => _userRepository.CleanupExpiredSessionsAsync();

    private static (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return (Convert.ToHexString(hashBytes), Convert.ToHexString(saltBytes));
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
        {
            return false;
        }

        var saltBytes = Convert.FromHexString(storedSalt);
        var computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(
            computedHashBytes,
            Convert.FromHexString(storedHash));
    }
}

public sealed class AuthResult
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public User? User { get; }
    public SessionInfo? Session { get; }

    private AuthResult(bool succeeded, string? error, User? user, SessionInfo? session)
    {
        Succeeded = succeeded;
        Error = error;
        User = user;
        Session = session;
    }

    public static AuthResult Failure(string message) => new(false, message, null, null);

    public static AuthResult Success(User user, SessionInfo session) =>
        new(true, null, user, session);
}
