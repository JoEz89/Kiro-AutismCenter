namespace AutismCenter.Application.Features.Products.Queries.Admin.GetInventoryReport;

public record GetInventoryReportResponse(
    IEnumerable<InventoryReportItem> Items,
    InventoryReportSummary Summary
);

public record InventoryReportItem(
    Guid ProductId,
    string NameEn,
    string NameAr,
    string ProductSku,
    string CategoryNameEn,
    string CategoryNameAr,
    int StockQuantity,
    decimal Price,
    string Currency,
    decimal InventoryValue,
    bool IsActive,
    bool IsLowStock,
    DateTime LastUpdated
);

public record InventoryReportSummary(
    int TotalProducts,
    int ActiveProducts,
    int InactiveProducts,
    int LowStockProducts,
    int OutOfStockProducts,
    decimal TotalInventoryValue,
    decimal AverageStockQuantity
);