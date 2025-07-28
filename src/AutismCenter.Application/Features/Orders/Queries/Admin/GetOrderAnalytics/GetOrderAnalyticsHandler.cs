using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrderAnalytics;

public class GetOrderAnalyticsHandler : IRequestHandler<GetOrderAnalyticsQuery, GetOrderAnalyticsResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public GetOrderAnalyticsHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<GetOrderAnalyticsResponse> Handle(GetOrderAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get orders within date range
        var orders = await _orderRepository.GetOrdersWithItemsByDateRangeAsync(startDate, endDate, cancellationToken);

        // Apply additional filters
        if (request.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        if (request.UserId.HasValue)
        {
            orders = orders.Where(o => o.UserId == request.UserId.Value);
        }

        var orderList = orders.ToList();

        // Calculate overview analytics
        var overview = CalculateOverviewAnalytics(orderList);

        // Calculate status breakdown
        var statusBreakdown = CalculateStatusBreakdown(orderList);

        // Calculate monthly trends
        var monthlyTrends = CalculateMonthlyTrends(orderList);

        // Calculate top customers
        var topCustomers = await CalculateTopCustomers(orderList, cancellationToken);

        // Calculate top products
        var topProducts = await CalculateTopProducts(orderList, cancellationToken);

        return new GetOrderAnalyticsResponse(
            overview,
            statusBreakdown,
            monthlyTrends,
            topCustomers,
            topProducts);
    }

    private static OrderOverviewAnalytics CalculateOverviewAnalytics(IList<Domain.Entities.Order> orders)
    {
        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount.Amount);
        var currency = orders.FirstOrDefault()?.TotalAmount.Currency ?? "BHD";
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        
        var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
        var completedOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
        var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
        var refundedOrders = orders.Count(o => o.Status == OrderStatus.Refunded);
        
        var refundRate = totalOrders > 0 ? (double)refundedOrders / totalOrders : 0;

        return new OrderOverviewAnalytics(
            totalOrders,
            totalRevenue,
            currency,
            averageOrderValue,
            pendingOrders,
            completedOrders,
            cancelledOrders,
            refundedOrders,
            (decimal)refundRate);
    }

    private static IEnumerable<OrderStatusAnalytics> CalculateStatusBreakdown(IList<Domain.Entities.Order> orders)
    {
        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount.Amount);

        return orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusAnalytics(
                g.Key,
                g.Count(),
                g.Sum(o => o.TotalAmount.Amount),
                totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0))
            .OrderByDescending(s => s.Count);
    }

    private static IEnumerable<MonthlyOrderAnalytics> CalculateMonthlyTrends(IList<Domain.Entities.Order> orders)
    {
        return orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyOrderAnalytics(
                g.Key.Year,
                g.Key.Month,
                g.Count(),
                g.Sum(o => o.TotalAmount.Amount),
                g.Count() > 0 ? g.Sum(o => o.TotalAmount.Amount) / g.Count() : 0))
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month);
    }

    private async Task<IEnumerable<TopCustomerAnalytics>> CalculateTopCustomers(
        IList<Domain.Entities.Order> orders, 
        CancellationToken cancellationToken)
    {
        var customerStats = orders
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount.Amount),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToList();

        var topCustomers = new List<TopCustomerAnalytics>();

        foreach (var customerStat in customerStats)
        {
            var user = await _userRepository.GetByIdAsync(customerStat.UserId, cancellationToken);
            if (user != null)
            {
                topCustomers.Add(new TopCustomerAnalytics(
                    customerStat.UserId,
                    $"{user.FirstName} {user.LastName}",
                    user.Email.Value,
                    customerStat.OrderCount,
                    customerStat.TotalSpent,
                    customerStat.LastOrderDate));
            }
        }

        return topCustomers;
    }

    private async Task<IEnumerable<ProductSalesAnalytics>> CalculateTopProducts(
        IList<Domain.Entities.Order> orders, 
        CancellationToken cancellationToken)
    {
        var productStats = orders
            .SelectMany(o => o.Items)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.UnitPrice.Amount * oi.Quantity),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        var topProducts = new List<ProductSalesAnalytics>();

        foreach (var productStat in productStats)
        {
            var product = await _productRepository.GetByIdAsync(productStat.ProductId, cancellationToken);
            if (product != null)
            {
                topProducts.Add(new ProductSalesAnalytics(
                    productStat.ProductId,
                    product.NameEn,
                    product.ProductSku,
                    productStat.QuantitySold,
                    productStat.Revenue,
                    productStat.OrderCount));
            }
        }

        return topProducts;
    }
}