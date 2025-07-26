using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateProgress;

public record UpdateProgressResponse(
    bool Success,
    string Message,
    EnrollmentDto? Enrollment = null,
    Dictionary<string, string[]>? Errors = null
);