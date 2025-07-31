using AutismCenter.Application.Common.Models;

namespace AutismCenter.Application.Features.Courses.Queries.Admin.GetCoursesAdmin;

public record GetCoursesAdminResponse(
    PagedResult<CourseAdminDto> Courses
);

public record CourseAdminDto(
    Guid Id,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int Duration,
    decimal Price,
    string Currency,

    string CourseCode,
    string? ThumbnailUrl,
    bool IsActive,
    int EnrollmentCount,
    int CompletionCount,
    decimal Revenue,
    DateTime CreatedAt,
    DateTime UpdatedAt
);