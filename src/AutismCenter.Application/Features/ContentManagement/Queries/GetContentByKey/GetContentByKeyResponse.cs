using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentByKey;

public record ContentTranslation(
    Guid Id,
    Language Language,
    string Content,
    string? Description,
    bool IsActive,
    string UpdatedBy,
    DateTime UpdatedAt
);

public record GetContentByKeyResponse(
    string Key,
    string Category,
    IEnumerable<ContentTranslation> Translations
);