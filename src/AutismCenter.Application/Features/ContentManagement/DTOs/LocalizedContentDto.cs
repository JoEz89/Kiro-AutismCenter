using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.DTOs;

public record LocalizedContentDto(
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

public record LocalizedContentSummaryDto(
    Guid Id,
    string Key,
    Language Language,
    string ContentPreview,
    string Category,
    bool IsActive,
    DateTime UpdatedAt
);

public record ContentTranslationStatusDto(
    string Key,
    string Category,
    bool HasEnglish,
    bool HasArabic,
    bool EnglishActive,
    bool ArabicActive,
    DateTime? LastUpdated
);