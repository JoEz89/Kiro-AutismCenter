using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Entities;

public class LocalizedContent : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public Language Language { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public string CreatedBy { get; private set; } = string.Empty;
    public string UpdatedBy { get; private set; } = string.Empty;

    private LocalizedContent() { } // For EF Core

    public LocalizedContent(
        string key,
        Language language,
        string content,
        string category,
        string? description = null,
        string createdBy = "system")
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Language = language;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Description = description;
        CreatedBy = createdBy;
    }

    public void UpdateContent(string content, string updatedBy)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description, string updatedBy)
    {
        Description = description;
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdateTimestamp();
    }
}