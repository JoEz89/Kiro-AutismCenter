using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;

public record UpdateLocalizedContentResponse(
    Guid Id,
    string Key,
    Language Language,
    string Content,
    string Category,
    string? Description,
    bool IsActive,
    DateTime UpdatedAt
);