using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.UpdateCourseAdmin;

public record UpdateCourseAdminCommand(
    Guid CourseId,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int Duration,
    decimal Price,
    string Currency,
    Guid? CategoryId,
    string? ThumbnailUrl,
    bool IsActive
) : IRequest<UpdateCourseAdminResponse>;