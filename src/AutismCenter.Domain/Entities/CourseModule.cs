using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class CourseModule : BaseEntity
{
    public Guid CourseId { get; private set; }
    public string TitleEn { get; private set; }
    public string TitleAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string VideoUrl { get; private set; }
    public int DurationInMinutes { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public Course Course { get; private set; } = null!;

    private CourseModule() { } // For EF Core

    private CourseModule(Guid courseId, string titleEn, string titleAr, string videoUrl,
                        int durationInMinutes, int order, string? descriptionEn = null, string? descriptionAr = null)
    {
        CourseId = courseId;
        TitleEn = titleEn;
        TitleAr = titleAr;
        DescriptionEn = descriptionEn;
        DescriptionAr = descriptionAr;
        VideoUrl = videoUrl;
        DurationInMinutes = durationInMinutes;
        Order = order;
        IsActive = true;
    }

    public static CourseModule Create(Guid courseId, string titleEn, string titleAr, string videoUrl,
                                     int durationInMinutes, int order, string? descriptionEn = null, string? descriptionAr = null)
    {
        if (string.IsNullOrWhiteSpace(titleEn))
            throw new ArgumentException("English title cannot be empty", nameof(titleEn));

        if (string.IsNullOrWhiteSpace(titleAr))
            throw new ArgumentException("Arabic title cannot be empty", nameof(titleAr));

        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("Video URL cannot be empty", nameof(videoUrl));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        if (order <= 0)
            throw new ArgumentException("Order must be positive", nameof(order));

        return new CourseModule(courseId, titleEn.Trim(), titleAr.Trim(), videoUrl.Trim(),
                               durationInMinutes, order, descriptionEn?.Trim(), descriptionAr?.Trim());
    }

    public void UpdateDetails(string titleEn, string titleAr, string videoUrl, int durationInMinutes,
                             string? descriptionEn = null, string? descriptionAr = null)
    {
        if (string.IsNullOrWhiteSpace(titleEn))
            throw new ArgumentException("English title cannot be empty", nameof(titleEn));

        if (string.IsNullOrWhiteSpace(titleAr))
            throw new ArgumentException("Arabic title cannot be empty", nameof(titleAr));

        if (string.IsNullOrWhiteSpace(videoUrl))
            throw new ArgumentException("Video URL cannot be empty", nameof(videoUrl));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        TitleEn = titleEn.Trim();
        TitleAr = titleAr.Trim();
        DescriptionEn = descriptionEn?.Trim();
        DescriptionAr = descriptionAr?.Trim();
        VideoUrl = videoUrl.Trim();
        DurationInMinutes = durationInMinutes;
        UpdateTimestamp();
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder <= 0)
            throw new ArgumentException("Order must be positive", nameof(newOrder));

        Order = newOrder;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();
    }

    public string GetTitle(bool isArabic) => isArabic ? TitleAr : TitleEn;

    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
}