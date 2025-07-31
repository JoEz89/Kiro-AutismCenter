namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting localized content list
/// </summary>
public class GetLocalizedContentListRequest
{
    /// <summary>
    /// Filter by content category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filter by language (en, ar)
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Search term for filtering content by key or content
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request model for creating localized content
/// </summary>
public class CreateLocalizedContentRequest
{
    /// <summary>
    /// Unique key for the content
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Content category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Content in English
    /// </summary>
    public string EnglishContent { get; set; } = string.Empty;

    /// <summary>
    /// Content in Arabic
    /// </summary>
    public string ArabicContent { get; set; } = string.Empty;

    /// <summary>
    /// Type of content (text, html, json)
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Whether the content is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating localized content
/// </summary>
public class UpdateLocalizedContentRequest
{
    /// <summary>
    /// Unique key for the content
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Content category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Content in English
    /// </summary>
    public string EnglishContent { get; set; } = string.Empty;

    /// <summary>
    /// Content in Arabic
    /// </summary>
    public string ArabicContent { get; set; } = string.Empty;

    /// <summary>
    /// Type of content (text, html, json)
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Whether the content is active
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Request model for bulk updating localized content
/// </summary>
public class BulkUpdateLocalizedContentRequest
{
    /// <summary>
    /// List of content updates
    /// </summary>
    public List<ContentUpdateItem> ContentUpdates { get; set; } = new();
}

/// <summary>
/// Individual content update item
/// </summary>
public class ContentUpdateItem
{
    /// <summary>
    /// Content ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique key for the content
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Content category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Content in English
    /// </summary>
    public string EnglishContent { get; set; } = string.Empty;

    /// <summary>
    /// Content in Arabic
    /// </summary>
    public string ArabicContent { get; set; } = string.Empty;

    /// <summary>
    /// Type of content (text, html, json)
    /// </summary>
    public string ContentType { get; set; } = "text";

    /// <summary>
    /// Whether the content is active
    /// </summary>
    public bool IsActive { get; set; }
}