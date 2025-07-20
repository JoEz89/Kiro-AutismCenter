using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.Application.Features.Products.Queries.GetProducts;

public record GetProductsResponse(
    PagedResult<ProductSummaryDto> Products
);