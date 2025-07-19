using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Course : BaseEntity
{
    public string TitleEn { get; private set; }
    public string TitleAr { get; private set; }
    public string DescriptionEn { get; private set; }
    public string DescriptionAr { get; private set; }
    public int DurationInMinutes { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public Money Price { get; private set; }
    public bool IsActive { get; private set; }
    public string CourseCode { get; private set; }

    // Navigation properties
    private readonly List<CourseModule> _modules = new();
    public IReadOnlyCollection<CourseModule> Modules => _modules.AsReadOnly();

    private readonly List<Enrollment> _enrollments = new();
    public IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    private Course() { } // For EF Core

    private Course(string titleEn, string titleAr, string descriptionEn, string descriptionAr,
                  int durationInMinutes, Money price, string courseCode)
    {
        TitleEn = titleEn;
        TitleAr = titleAr;
        DescriptionEn = descriptionEn;
        DescriptionAr = descriptionAr;
        DurationInMinutes = durationInMinutes;
        Price = price;
        CourseCode = courseCode;
        IsActive = true;
    }

    public static Course Create(string titleEn, string titleAr, string descriptionEn, string descriptionAr,
                               int durationInMinutes, Money price, string courseCode)
    {
        if (string.IsNullOrWhiteSpace(titleEn))
            throw new ArgumentException("English title cannot be empty", nameof(titleEn));

        if (string.IsNullOrWhiteSpace(titleAr))
            throw new ArgumentException("Arabic title cannot be empty", nameof(titleAr));

        if (string.IsNullOrWhiteSpace(descriptionEn))
            throw new ArgumentException("English description cannot be empty", nameof(descriptionEn));

        if (string.IsNullOrWhiteSpace(descriptionAr))
            throw new ArgumentException("Arabic description cannot be empty", nameof(descriptionAr));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        if (string.IsNullOrWhiteSpace(courseCode))
            throw new ArgumentException("Course code cannot be empty", nameof(courseCode));

        var course = new Course(titleEn.Trim(), titleAr.Trim(), descriptionEn.Trim(), descriptionAr.Trim(),
                               durationInMinutes, price, courseCode.Trim());

        course.AddDomainEvent(new CourseCreatedEvent(course.Id, course.TitleEn));

        return course;
    }

    public void UpdateDetails(string titleEn, string titleAr, string descriptionEn, string descriptionAr,
                             int durationInMinutes, Money price)
    {
        if (string.IsNullOrWhiteSpace(titleEn))
            throw new ArgumentException("English title cannot be empty", nameof(titleEn));

        if (string.IsNullOrWhiteSpace(titleAr))
            throw new ArgumentException("Arabic title cannot be empty", nameof(titleAr));

        if (string.IsNullOrWhiteSpace(descriptionEn))
            throw new ArgumentException("English description cannot be empty", nameof(descriptionEn));

        if (string.IsNullOrWhiteSpace(descriptionAr))
            throw new ArgumentException("Arabic description cannot be empty", nameof(descriptionAr));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        TitleEn = titleEn.Trim();
        TitleAr = titleAr.Trim();
        DescriptionEn = descriptionEn.Trim();
        DescriptionAr = descriptionAr.Trim();
        DurationInMinutes = durationInMinutes;
        Price = price;
        UpdateTimestamp();

        AddDomainEvent(new CourseUpdatedEvent(Id));
    }

    public void SetThumbnail(string thumbnailUrl)
    {
        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("Thumbnail URL cannot be empty", nameof(thumbnailUrl));

        ThumbnailUrl = thumbnailUrl;
        UpdateTimestamp();
    }

    public void AddModule(CourseModule module)
    {
        if (_modules.Any(m => m.Order == module.Order))
            throw new InvalidOperationException($"Module with order {module.Order} already exists");

        _modules.Add(module);
        UpdateTimestamp();
    }

    public void RemoveModule(Guid moduleId)
    {
        var module = _modules.FirstOrDefault(m => m.Id == moduleId);
        if (module != null)
        {
            _modules.Remove(module);
            ReorderModules();
            UpdateTimestamp();
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();

        AddDomainEvent(new CourseActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();

        AddDomainEvent(new CourseDeactivatedEvent(Id));
    }

    private void ReorderModules()
    {
        var orderedModules = _modules.OrderBy(m => m.Order).ToList();
        for (int i = 0; i < orderedModules.Count; i++)
        {
            orderedModules[i].UpdateOrder(i + 1);
        }
    }

    public string GetTitle(bool isArabic) => isArabic ? TitleAr : TitleEn;

    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;

    public int GetModuleCount() => _modules.Count;

    public bool HasModules() => _modules.Any();
}