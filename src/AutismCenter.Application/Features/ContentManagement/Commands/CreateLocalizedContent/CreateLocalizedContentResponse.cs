using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;

public record CreateLocalizedContentResponse(
    Guid Id,
    string Key,
    Language Language,
    string Content,
    string Category,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);