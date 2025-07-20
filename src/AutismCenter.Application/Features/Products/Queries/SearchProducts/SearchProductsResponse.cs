using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Products.Common;

namespace AutismCenter.Application.Features.Products.Queries.SearchProducts;

public record SearchProductsResponse(
    PagedResult<ProductSummaryDto> Products,
    string SearchTerm
);