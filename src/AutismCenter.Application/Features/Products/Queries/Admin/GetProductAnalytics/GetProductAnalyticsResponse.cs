namespace AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;

public record GetProductAnalyticsResponse(
    ProductOverviewAnalytics Overview,
    IEnumerable<ProductPerformanceAnalytics> TopSellingProducts,
    IEnumerable<ProductPerformanceAnalytics> LowStockProducts,
    IEnumerable<CategoryAnalytics> CategoryBreakdown,
    IEnumerable<MonthlyProductAnalytics> MonthlyTrends
);

public record ProductOverviewAnalytics(
    int TotalProducts,
    int ActiveProducts,
    int InactiveProducts,
    int OutOfStockProducts,
    int LowStockProducts,
    decimal TotalInventoryValue,
    decimal AverageProductPrice
);

public record ProductPerformanceAnalytics(
    Guid ProductId,
    string NameEn,
    string NameAr,
    string ProductSku,
    int StockQuantity,
    int TotalSold,
    decimal Revenue,
    decimal Price,
    string Currency,
    DateTime LastSold
);

public record CategoryAnalytics(
    Guid CategoryId,
    string NameEn,
    string NameAr,
    int ProductCount,
    int TotalSold,
    decimal Revenue,
    decimal AveragePrice
);

public record MonthlyProductAnalytics(
    int Year,
    int Month,
    int ProductsSold,
    decimal Revenue,
    int OrderCount
);