using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsQuery(
    string SearchTerm,
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    bool? IsActive = null,
    bool? InStockOnly = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<SearchProductsResponse>;