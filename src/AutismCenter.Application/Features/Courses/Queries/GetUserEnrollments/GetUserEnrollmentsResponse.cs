using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;

public record GetUserEnrollmentsResponse(
    bool Success,
    string Message,
    IEnumerable<EnrollmentDto> Enrollments,
    Dictionary<string, string[]>? Errors = null
);