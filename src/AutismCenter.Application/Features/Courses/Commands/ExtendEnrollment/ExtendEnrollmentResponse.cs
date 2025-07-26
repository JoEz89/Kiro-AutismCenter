using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;

public record ExtendEnrollmentResponse(
    bool Success,
    string Message,
    EnrollmentDto? Enrollment = null,
    Dictionary<string, string[]>? Errors = null
);