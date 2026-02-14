namespace MiniDrive.Sharing.DTOs;

/// <summary>
/// Request DTO for accessing a public share.
/// </summary>
public class AccessPublicShareRequest
{
    /// <summary>
    /// Password for the share if it's protected.
    /// </summary>
    public string? Password { get; set; }
}
