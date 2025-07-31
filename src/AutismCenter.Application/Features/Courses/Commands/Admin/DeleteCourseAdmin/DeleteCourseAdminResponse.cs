namespace AutismCenter.Application.Features.Courses.Commands.Admin.DeleteCourseAdmin;

public record DeleteCourseAdminResponse(
    Guid Id,
    bool IsDeleted,
    DateTime DeletedAt
);