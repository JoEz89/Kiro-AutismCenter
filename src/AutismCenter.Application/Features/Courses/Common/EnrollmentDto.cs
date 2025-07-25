using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Courses.Common;

public record EnrollmentDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    string CourseTitle,
    string CourseTitleAr,
    DateTime EnrollmentDate,
    DateTime ExpiryDate,
    int ProgressPercentage,
    DateTime? CompletionDate,
    string? CertificateUrl,
    bool IsActive,
    bool IsExpired,
    bool IsCompleted,
    int DaysRemaining
)
{
    public static EnrollmentDto FromEntity(Enrollment enrollment)
    {
        if (enrollment == null)
            throw new ArgumentNullException(nameof(enrollment));

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.UserId,
            enrollment.CourseId,
            enrollment.Course?.TitleEn ?? string.Empty,
            enrollment.Course?.TitleAr ?? string.Empty,
            enrollment.EnrollmentDate,
            enrollment.ExpiryDate,
            enrollment.ProgressPercentage,
            enrollment.CompletionDate,
            enrollment.CertificateUrl,
            enrollment.IsActive,
            enrollment.IsExpired(),
            enrollment.IsCompleted(),
            enrollment.GetDaysRemaining()
        );
    }

    public string GetCourseTitle(bool isArabic) => isArabic ? CourseTitleAr : CourseTitle;
}