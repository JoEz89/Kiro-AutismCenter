using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid Id,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int DurationInMinutes,
    decimal Price,
    string Currency,
    string? ThumbnailUrl = null
) : IRequest<UpdateCourseResponse>;