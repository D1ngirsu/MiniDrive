namespace MiniDrive.Common.Jwt;

/// <summary>
/// Options used to generate and validate JWT tokens.
/// </summary>
public sealed class JwtOptions
{
    public const string ConfigurationSectionName = "Jwt";

    /// <summary>
    /// Application issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Intended audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key. Should be at least 32 characters for HS256.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Lifetime of an access token.
    /// </summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromHours(12);

    /// <summary>
    /// Lifetime of a refresh token (if used).
    /// </summary>
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);

    public bool IsValid(out string? error)
    {
        if (string.IsNullOrWhiteSpace(SigningKey) || SigningKey.Length < 32)
        {
            error = "SigningKey must be configured and at least 32 characters long.";
            return false;
        }

        if (AccessTokenLifetime <= TimeSpan.Zero)
        {
            error = "AccessTokenLifetime must be greater than zero.";
            return false;
        }

        error = null;
        return true;
    }
}
