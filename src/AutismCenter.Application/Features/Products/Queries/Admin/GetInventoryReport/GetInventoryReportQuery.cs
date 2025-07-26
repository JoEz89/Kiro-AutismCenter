using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetInventoryReport;

public record GetInventoryReportQuery(
    Guid? CategoryId = null,
    bool? IsActive = null,
    bool? LowStockOnly = null,
    int LowStockThreshold = 10
) : IRequest<GetInventoryReportResponse>;