using AutismCenter.Application.Common.Models;
using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    bool? IsActive = null,
    bool? InStockOnly = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    bool SortDescending = false
) : IRequest<GetProductsResponse>;