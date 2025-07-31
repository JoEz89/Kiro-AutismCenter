using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutismCenter.Application.Features.Courses.Queries.GetCourses;
using AutismCenter.Application.Features.Courses.Queries.GetCourseById;
using AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;
using AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;
using AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;
using AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;
using AutismCenter.Application.Features.Courses.Commands.EnrollUser;
using AutismCenter.Application.Features.Courses.Commands.UpdateProgress;
using AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;
using AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : BaseController
{
    /// <summary>
    /// Get all available courses with optional search and filtering
    /// </summary>
    /// <param name="activeOnly">Filter to show only active courses (default: true)</param>
    /// <param name="searchTerm">Search term to filter courses by title or description</param>
    /// <returns>List of courses matching the criteria</returns>
    [HttpGet]
    public async Task<ActionResult<GetCoursesResponse>> GetCourses(
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = new GetCoursesQuery(activeOnly, searchTerm);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving courses");
        }
    }

    /// <summary>
    /// Get detailed information about a specific course
    /// </summary>
    /// <param name="id">The unique identifier of the course</param>
    /// <returns>Detailed course information</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetCourseByIdResponse>> GetCourseById(Guid id)
    {
        try
        {
            var query = new GetCourseByIdQuery(id);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                if (result.Errors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving the course");
        }
    }

    /// <summary>
    /// Enroll the authenticated user in a specific course
    /// </summary>
    /// <param name="courseId">The unique identifier of the course to enroll in</param>
    /// <param name="request">Enrollment request details</param>
    /// <returns>Enrollment confirmation details</returns>
    [HttpPost("{courseId:guid}/enroll")]
    [Authorize]
    public async Task<ActionResult<EnrollUserResponse>> EnrollInCourse(
        Guid courseId, 
        [FromBody] EnrollmentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new EnrollUserCommand(userId, courseId, request.ValidityDays ?? 30);
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                if (result.Errors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while enrolling in the course");
        }
    }

    /// <summary>
    /// Get all enrollments for the authenticated user
    /// </summary>
    /// <param name="includeExpired">Include expired enrollments in the results (default: false)</param>
    /// <param name="includeCompleted">Include completed enrollments in the results (default: true)</param>
    /// <returns>List of user's course enrollments</returns>
    [HttpGet("enrollments")]
    [Authorize]
    public async Task<ActionResult<GetUserEnrollmentsResponse>> GetUserEnrollments(
        [FromQuery] bool includeExpired = false,
        [FromQuery] bool includeCompleted = true)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new GetUserEnrollmentsQuery(userId, includeExpired, includeCompleted);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving user enrollments");
        }
    }

    /// <summary>
    /// Get progress information for a specific enrollment
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <returns>Detailed progress information</returns>
    [HttpGet("enrollments/{enrollmentId:guid}/progress")]
    [Authorize]
    public async Task<ActionResult<GetEnrollmentProgressResponse>> GetEnrollmentProgress(Guid enrollmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new GetEnrollmentProgressQuery(enrollmentId);
            var result = await Mediator.Send(query);

            if (!result.Success)
            {
                if (result.Errors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.Errors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving enrollment progress");
        }
    }

    /// <summary>
    /// Update progress for a specific enrollment
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <param name="request">Progress update details</param>
    /// <returns>Updated progress information</returns>
    [HttpPut("enrollments/{enrollmentId:guid}/progress")]
    [Authorize]
    public async Task<ActionResult<UpdateProgressResponse>> UpdateProgress(
        Guid enrollmentId, 
        [FromBody] UpdateProgressRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new UpdateProgressCommand(
                enrollmentId, 
                request.ModuleId, 
                request.ProgressPercentage,
                request.TimeSpentMinutes * 60); // Convert minutes to seconds
            
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                if (result.Errors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.Errors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating progress");
        }
    }

    /// <summary>
    /// Get completion status for a specific course enrollment
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <returns>Course completion status and certificate information</returns>
    [HttpGet("enrollments/{enrollmentId:guid}/completion")]
    [Authorize]
    public async Task<ActionResult<GetCourseCompletionStatusResponse>> GetCompletionStatus(Guid enrollmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new GetCourseCompletionStatusQuery(enrollmentId);
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.ValidationErrors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving completion status");
        }
    }

    /// <summary>
    /// Generate a certificate for a completed course
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <returns>Certificate generation result</returns>
    [HttpPost("enrollments/{enrollmentId:guid}/certificate")]
    [Authorize]
    public async Task<ActionResult<GenerateCertificateResponse>> GenerateCertificate(Guid enrollmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new GenerateCertificateCommand(enrollmentId);
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.ValidationErrors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while generating the certificate");
        }
    }

    /// <summary>
    /// Download a certificate for a completed course
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <returns>Certificate file download</returns>
    [HttpGet("enrollments/{enrollmentId:guid}/certificate/download")]
    [Authorize]
    public async Task<ActionResult> DownloadCertificate(Guid enrollmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var query = new DownloadCertificateQuery(enrollmentId, userId);
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                if (result.ValidationErrors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.ValidationErrors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            if (result.CertificateData == null || string.IsNullOrEmpty(result.FileName))
            {
                return NotFound("Certificate file not found");
            }

            return File(result.CertificateData, result.ContentType!, result.FileName);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while downloading the certificate");
        }
    }

    /// <summary>
    /// Extend an enrollment's validity period
    /// </summary>
    /// <param name="enrollmentId">The unique identifier of the enrollment</param>
    /// <param name="request">Extension request details</param>
    /// <returns>Updated enrollment information</returns>
    [HttpPost("enrollments/{enrollmentId:guid}/extend")]
    [Authorize]
    public async Task<ActionResult<ExtendEnrollmentResponse>> ExtendEnrollment(
        Guid enrollmentId, 
        [FromBody] ExtendEnrollmentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized("User ID not found in token");
            }

            var command = new ExtendEnrollmentCommand(enrollmentId, request.AdditionalDays);
            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                if (result.Errors?.ContainsKey("NotFound") == true)
                {
                    return NotFound(result);
                }
                if (result.Errors?.ContainsKey("Forbidden") == true)
                {
                    return Forbid(result.Message);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while extending the enrollment");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

// Request DTOs
public class EnrollmentRequest
{
    public int? ValidityDays { get; set; } = 30;
}

public class UpdateProgressRequest
{
    public Guid ModuleId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TimeSpentMinutes { get; set; }
}

public class ExtendEnrollmentRequest
{
    public int AdditionalDays { get; set; }
}