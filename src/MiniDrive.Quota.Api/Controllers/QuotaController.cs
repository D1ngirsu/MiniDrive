using Microsoft.AspNetCore.Mvc;
using MiniDrive.Quota.Services;

namespace MiniDrive.Quota.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotaController : ControllerBase
{
    private readonly IQuotaService _quotaService;

    public QuotaController(IQuotaService quotaService)
    {
        _quotaService = quotaService;
    }

    /// <summary>
    /// Gets quota information for a user.
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetQuota(Guid userId)
    {
        var quota = await _quotaService.GetQuotaAsync(userId);
        if (quota == null)
        {
            return NotFound(new { error = "Quota not found." });
        }

        return Ok(new
        {
            userId = quota.UserId,
            usedBytes = quota.UsedBytes,
            limitBytes = quota.LimitBytes,
            availableBytes = quota.AvailableBytes
        });
    }

    /// <summary>
    /// Checks if a user can upload a file of the specified size.
    /// </summary>
    [HttpGet("{userId}/can-upload")]
    public async Task<IActionResult> CanUpload(Guid userId, [FromQuery] long fileSize)
    {
        var canUpload = await _quotaService.CanUploadAsync(userId, fileSize);
        return Ok(new { canUpload });
    }

    /// <summary>
    /// Increases the used storage for a user.
    /// </summary>
    [HttpPost("{userId}/increase")]
    public async Task<IActionResult> Increase(Guid userId, [FromBody] IncreaseRequest request)
    {
        var success = await _quotaService.IncreaseAsync(userId, request.Bytes);
        if (!success)
        {
            return BadRequest(new { error = "Failed to increase quota." });
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Decreases the used storage for a user.
    /// </summary>
    [HttpPost("{userId}/decrease")]
    public async Task<IActionResult> Decrease(Guid userId, [FromBody] DecreaseRequest request)
    {
        var success = await _quotaService.DecreaseAsync(userId, request.Bytes);
        if (!success)
        {
            return BadRequest(new { error = "Failed to decrease quota." });
        }

        return Ok(new { success = true });
    }
}

public class IncreaseRequest
{
    public long Bytes { get; set; }
}

public class DecreaseRequest
{
    public long Bytes { get; set; }
}

