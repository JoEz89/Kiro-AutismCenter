using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.Admin.ExportProducts;

public record ExportProductsQuery(
    Guid? CategoryId,
    bool? IsActive,
    bool LowStockOnly,
    string Format
) : IRequest<ExportProductsResponse>;