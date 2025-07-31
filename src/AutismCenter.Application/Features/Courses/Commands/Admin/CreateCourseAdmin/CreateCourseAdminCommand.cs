using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.Admin.CreateCourseAdmin;

public record CreateCourseAdminCommand(
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int Duration,
    decimal Price,
    string Currency,
    Guid? CategoryId,
    string CourseCode,
    string? ThumbnailUrl,
    bool IsActive
) : IRequest<CreateCourseAdminResponse>;