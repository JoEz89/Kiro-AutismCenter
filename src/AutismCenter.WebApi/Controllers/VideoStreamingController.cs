using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;
using AutismCenter.Application.Features.Courses.Commands.EndVideoSession;
using System.Security.Claims;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoStreamingController : BaseController
{
    public VideoStreamingController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Generates a secure, time-limited URL for video streaming
    /// </summary>
    /// <param name="moduleId">The ID of the course module containing the video</param>
    /// <param name="expirationMinutes">How long the URL should remain valid (default: 60 minutes, max: 120)</param>
    /// <returns>Secure streaming URL with session information</returns>
    [HttpPost("secure-url/{moduleId:guid}")]
    public async Task<ActionResult<GetSecureVideoUrlResponse>> GetSecureVideoUrl(
        Guid moduleId, 
        [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            // Validate expiration minutes (security constraint)
            if (expirationMinutes < 1 || expirationMinutes > 120)
            {
                return BadRequest("Expiration minutes must be between 1 and 120");
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new GetSecureVideoUrlQuery(userId, moduleId, expirationMinutes);
            var result = await Mediator.Send(query);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while generating the secure video URL");
        }
    }

    /// <summary>
    /// Ends an active video streaming session
    /// </summary>
    /// <param name="sessionId">The unique identifier for the streaming session</param>
    [HttpPost("end-session")]
    public async Task<ActionResult> EndVideoSession([FromBody] EndVideoSessionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                return BadRequest("Session ID is required");
            }

            var command = new EndVideoSessionCommand(request.SessionId);
            await Mediator.Send(command);

            return Ok(new { message = "Video session ended successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while ending the video session");
        }
    }

    /// <summary>
    /// Validates if the current user has access to a specific video
    /// </summary>
    /// <param name="moduleId">The ID of the course module containing the video</param>
    [HttpGet("validate-access/{moduleId:guid}")]
    public async Task<ActionResult<VideoAccessValidationResponse>> ValidateVideoAccess(Guid moduleId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            // This would typically use a separate query/service
            // For now, we'll use the video access service directly through dependency injection
            // In a full implementation, you'd create a separate query handler
            
            return Ok(new VideoAccessValidationResponse
            {
                HasAccess = false, // Placeholder - would be implemented with proper validation
                Reason = "Not implemented in this demo",
                DaysRemaining = 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while validating video access");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class EndVideoSessionRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class VideoAccessValidationResponse
{
    public bool HasAccess { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}