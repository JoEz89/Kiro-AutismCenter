namespace AutismCenter.Application.Features.Courses.Commands.Admin.CreateCourseAdmin;

public record CreateCourseAdminResponse(
    Guid Id,
    string TitleEn,
    string TitleAr,
    string CourseCode,
    bool IsActive,
    DateTime CreatedAt
);