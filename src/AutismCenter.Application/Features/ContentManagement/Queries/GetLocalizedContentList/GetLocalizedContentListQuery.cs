using AutismCenter.Domain.Enums;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContentList;

public record GetLocalizedContentListQuery(
    string? Category = null,
    Language? Language = null,
    bool? IsActive = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<GetLocalizedContentListResponse>;