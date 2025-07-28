using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;

public record GetOrderAnalyticsResponse(
    OrderOverviewAnalytics Overview,
    IEnumerable<OrderStatusAnalytics> StatusBreakdown,
    IEnumerable<MonthlyOrderAnalytics> MonthlyTrends,
    IEnumerable<TopCustomerAnalytics> TopCustomers,
    IEnumerable<ProductSalesAnalytics> TopProducts
);

public record OrderOverviewAnalytics(
    int TotalOrders,
    decimal TotalRevenue,
    string Currency,
    decimal AverageOrderValue,
    int PendingOrders,
    int CompletedOrders,
    int CancelledOrders,
    int RefundedOrders,
    decimal RefundRate
);

public record OrderStatusAnalytics(
    OrderStatus Status,
    int Count,
    decimal Revenue,
    double Percentage
);

public record MonthlyOrderAnalytics(
    int Year,
    int Month,
    int OrderCount,
    decimal Revenue,
    decimal AverageOrderValue
);

public record TopCustomerAnalytics(
    Guid UserId,
    string CustomerName,
    string Email,
    int OrderCount,
    decimal TotalSpent,
    DateTime LastOrderDate
);

public record ProductSalesAnalytics(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int QuantitySold,
    decimal Revenue,
    int OrderCount
);