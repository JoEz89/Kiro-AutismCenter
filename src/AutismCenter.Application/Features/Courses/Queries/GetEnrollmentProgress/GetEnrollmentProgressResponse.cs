using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;

public record GetEnrollmentProgressResponse(
    bool Success,
    string Message,
    EnrollmentDto? Enrollment = null,
    IEnumerable<ModuleProgressDto>? ModuleProgress = null,
    Dictionary<string, string[]>? Errors = null
);