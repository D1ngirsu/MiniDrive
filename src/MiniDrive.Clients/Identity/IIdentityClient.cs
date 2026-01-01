namespace MiniDrive.Clients.Identity;

/// <summary>
/// Client interface for Identity service operations.
/// </summary>
public interface IIdentityClient
{
    /// <summary>
    /// Validates a session token and returns user information.
    /// </summary>
    Task<UserInfo?> ValidateSessionAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// User information returned from Identity service.
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

