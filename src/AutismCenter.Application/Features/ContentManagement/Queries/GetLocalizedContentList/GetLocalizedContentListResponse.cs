using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContentList;

public record LocalizedContentListItem(
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

public record GetLocalizedContentListResponse(
    IEnumerable<LocalizedContentListItem> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);