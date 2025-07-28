using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContent;

public record GetLocalizedContentResponse(
    Guid Id,
    string Key,
    Language Language,
    string Content,
    string Category,
    string? Description,
    bool IsActive,
    string CreatedBy,
    string UpdatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);