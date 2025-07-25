using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Courses.Common;

public record CourseDto(
    Guid Id,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int DurationInMinutes,
    string? ThumbnailUrl,
    decimal Price,
    string Currency,
    bool IsActive,
    string CourseCode,
    int ModuleCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static CourseDto FromEntity(Course course)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        return new CourseDto(
            course.Id,
            course.TitleEn,
            course.TitleAr,
            course.DescriptionEn,
            course.DescriptionAr,
            course.DurationInMinutes,
            course.ThumbnailUrl,
            course.Price?.Amount ?? 0,
            course.Price?.Currency ?? "USD",
            course.IsActive,
            course.CourseCode,
            course.GetModuleCount(),
            course.CreatedAt,
            course.UpdatedAt
        );
    }

    public string GetTitle(bool isArabic) => isArabic ? TitleAr : TitleEn;
    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
}