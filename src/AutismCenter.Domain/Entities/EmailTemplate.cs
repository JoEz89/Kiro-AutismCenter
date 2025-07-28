using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public string TemplateKey { get; private set; } = string.Empty;
    public Language Language { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string CreatedBy { get; private set; } = string.Empty;
    public string UpdatedBy { get; private set; } = string.Empty;

    private EmailTemplate() { } // For EF Core

    public EmailTemplate(
        string templateKey,
        Language language,
        string subject,
        string body,
        string? description = null,
        string createdBy = "system")
    {
        TemplateKey = templateKey ?? throw new ArgumentNullException(nameof(templateKey));
        Language = language;
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Description = description;
        CreatedBy = createdBy;
    }

    public void UpdateTemplate(string subject, string body, string updatedBy)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
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