namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting courses with admin details
/// </summary>
public class GetCoursesAdminRequest
{
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Search term for filtering courses by title or description
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort by field (title, createdAt, enrollmentCount, etc.)
    /// </summary>
    public string SortBy { get; set; } = "createdAt";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Request model for course analytics
/// </summary>
public class GetCourseAnalyticsRequest
{
    /// <summary>
    /// Start date for analytics period
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for analytics period
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Request model for exporting courses
/// </summary>
public class ExportCoursesRequest
{
    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Export format (CSV, Excel)
    /// </summary>
    public string Format { get; set; } = "CSV";
}

/// <summary>
/// Request model for creating a course
/// </summary>
public class CreateCourseAdminRequest
{
    /// <summary>
    /// Course title in English
    /// </summary>
    public string TitleEn { get; set; } = string.Empty;

    /// <summary>
    /// Course title in Arabic
    /// </summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// Course description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Course description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Course duration in minutes
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Course price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Course thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Whether the course is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating a course
/// </summary>
public class UpdateCourseAdminRequest
{
    /// <summary>
    /// Course title in English
    /// </summary>
    public string TitleEn { get; set; } = string.Empty;

    /// <summary>
    /// Course title in Arabic
    /// </summary>
    public string TitleAr { get; set; } = string.Empty;

    /// <summary>
    /// Course description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Course description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Course duration in minutes
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Course price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Course thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Whether the course is active
    /// </summary>
    public bool IsActive { get; set; }
}