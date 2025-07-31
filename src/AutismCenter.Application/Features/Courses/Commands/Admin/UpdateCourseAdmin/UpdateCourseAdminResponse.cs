namespace AutismCenter.Application.Features.Courses.Commands.Admin.UpdateCourseAdmin;

public record UpdateCourseAdminResponse(
    Guid Id,
    string TitleEn,
    string TitleAr,
    bool IsActive,
    DateTime UpdatedAt
);