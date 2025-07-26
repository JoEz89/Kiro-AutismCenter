using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.EnrollUser;

public record EnrollUserResponse(
    bool Success,
    string Message,
    EnrollmentDto? Enrollment = null,
    Dictionary<string, string[]>? Errors = null
);