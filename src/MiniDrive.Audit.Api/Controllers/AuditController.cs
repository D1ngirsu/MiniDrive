using Microsoft.AspNetCore.Mvc;
using MiniDrive.Audit.Services;

namespace MiniDrive.Audit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Logs an audit action.
    /// </summary>
    [HttpPost("log")]
    public async Task<IActionResult> LogAction([FromBody] LogActionRequest request)
    {
        await _auditService.LogActionAsync(
            request.UserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.IsSuccess,
            request.Details,
            request.ErrorMessage,
            request.IpAddress,
            request.UserAgent);

        return Ok(new { success = true });
    }
}

public class LogActionRequest
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

