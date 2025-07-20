using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Products.Common;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.GetPagedAsync(
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

        return new GetProductsResponse(pagedResult);
    }
}