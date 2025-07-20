using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.SearchProducts;

public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, SearchProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<SearchProductsResponse> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.SearchPagedAsync(
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.IsActive,
            request.InStockOnly,
            request.MinPrice,
            request.MaxPrice,
            request.SortBy,
            request.SortDescending,
            cancellationToken
        );

        var productDtos = products.Select(ProductSummaryDto.FromEntity);

        var pagedResult = PagedResult<ProductSummaryDto>.Create(
            productDtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        return new SearchProductsResponse(pagedResult, request.SearchTerm);
    }
}