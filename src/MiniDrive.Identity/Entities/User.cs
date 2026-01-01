namespace MiniDrive.Identity.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Session
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
}

// Keep SessionInfo for backward compatibility in services
public sealed class SessionInfo
{
    public string Token { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public string? UserAgent { get; init; }
    public string? IpAddress { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public static SessionInfo FromSession(Session session)
    {
        return new SessionInfo
        {
            Token = session.Token,
            UserId = session.UserId,
            CreatedAtUtc = session.CreatedAtUtc,
            ExpiresAtUtc = session.ExpiresAtUtc,
            UserAgent = session.UserAgent,
            IpAddress = session.IpAddress
        };
    }
}
