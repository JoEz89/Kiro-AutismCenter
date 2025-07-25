using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int DurationInMinutes,
    decimal Price,
    string Currency,
    string CourseCode,
    string? ThumbnailUrl = null
) : IRequest<CreateCourseResponse>;