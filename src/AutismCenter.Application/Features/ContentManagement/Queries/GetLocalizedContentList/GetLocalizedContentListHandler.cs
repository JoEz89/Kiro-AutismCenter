using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContentList;

public class GetLocalizedContentListHandler : IRequestHandler<GetLocalizedContentListQuery, GetLocalizedContentListResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;

    public GetLocalizedContentListHandler(ILocalizedContentRepository localizedContentRepository)
    {
        _localizedContentRepository = localizedContentRepository;
    }

    public async Task<GetLocalizedContentListResponse> Handle(GetLocalizedContentListQuery request, CancellationToken cancellationToken)
    {
        // Get all content based on filters
        var allContent = await _localizedContentRepository.GetAllAsync(request.Language);
        
        // Apply filters
        var filteredContent = allContent.AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
        {
            filteredContent = filteredContent.Where(c => c.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (request.IsActive.HasValue)
        {
            filteredContent = filteredContent.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filteredContent = filteredContent.Where(c => 
                c.Key.ToLower().Contains(searchTerm) ||
                c.Content.ToLower().Contains(searchTerm) ||
                (c.Description != null && c.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        filteredContent = request.SortBy?.ToLower() switch
        {
            "key" => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.Key)
                : filteredContent.OrderBy(c => c.Key),
            "language" => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.Language)
                : filteredContent.OrderBy(c => c.Language),
            "category" => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.Category)
                : filteredContent.OrderBy(c => c.Category),
            "isactive" => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.IsActive)
                : filteredContent.OrderBy(c => c.IsActive),
            "updatedat" => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.UpdatedAt)
                : filteredContent.OrderBy(c => c.UpdatedAt),
            _ => request.SortDescending 
                ? filteredContent.OrderByDescending(c => c.CreatedAt)
                : filteredContent.OrderBy(c => c.CreatedAt)
        };

        var totalCount = filteredContent.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        // Apply pagination
        var pagedContent = filteredContent
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedContent.Select(c => new LocalizedContentListItem(
            c.Id,
            c.Key,
            c.Language,
            c.Content.Length > 100 ? c.Content.Substring(0, 100) + "..." : c.Content,
            c.Category,
            c.Description,
            c.IsActive,
            c.CreatedBy,
            c.UpdatedBy,
            c.CreatedAt,
            c.UpdatedAt));

        return new GetLocalizedContentListResponse(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}