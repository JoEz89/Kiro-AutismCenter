using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;

public record BulkUpdateStockCommand(
    IEnumerable<StockUpdateItem> StockUpdates
) : IRequest<BulkUpdateStockResponse>;

public record StockUpdateItem(
    Guid ProductId,
    int NewQuantity
);