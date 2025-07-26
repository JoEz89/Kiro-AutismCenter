namespace AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;

public record BulkUpdateStockResponse(
    int TotalUpdated,
    int TotalFailed,
    IEnumerable<StockUpdateResult> Results
);

public record StockUpdateResult(
    Guid ProductId,
    bool Success,
    string? ErrorMessage,
    int? OldQuantity,
    int? NewQuantity
);