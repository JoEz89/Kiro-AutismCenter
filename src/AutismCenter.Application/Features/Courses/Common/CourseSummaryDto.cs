using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Courses.Common;

public record CourseSummaryDto(
    Guid Id,
    string TitleEn,
    string TitleAr,
    string DescriptionEn,
    string DescriptionAr,
    int DurationInMinutes,
    string? ThumbnailUrl,
    decimal Price,
    string Currency,
    string CourseCode,
    int ModuleCount,
    bool IsActive
)
{
    public static CourseSummaryDto FromEntity(Course course)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        return new CourseSummaryDto(
            course.Id,
            course.TitleEn,
            course.TitleAr,
            course.DescriptionEn,
            course.DescriptionAr,
            course.DurationInMinutes,
            course.ThumbnailUrl,
            course.Price?.Amount ?? 0,
            course.Price?.Currency ?? "USD",
            course.CourseCode,
            course.GetModuleCount(),
            course.IsActive
        );
    }

    public string GetTitle(bool isArabic) => isArabic ? TitleAr : TitleEn;
    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
}