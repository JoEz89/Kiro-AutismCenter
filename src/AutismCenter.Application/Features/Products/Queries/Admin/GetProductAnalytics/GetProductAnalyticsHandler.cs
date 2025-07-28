using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;

public class GetProductAnalyticsHandler : IRequestHandler<GetProductAnalyticsQuery, GetProductAnalyticsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProductAnalyticsHandler(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<GetProductAnalyticsResponse> Handle(GetProductAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get all products
        var products = await _productRepository.GetAllAsync(cancellationToken);
        if (request.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        // Get orders within date range
        var orders = await _orderRepository.GetOrdersWithItemsByDateRangeAsync(startDate, endDate, cancellationToken);

        // Calculate overview analytics
        var overview = CalculateOverviewAnalytics(products);

        // Calculate top selling products
        var topSellingProducts = CalculateTopSellingProducts(products, orders);

        // Calculate low stock products
        var lowStockProducts = CalculateLowStockProducts(products);

        // Calculate category breakdown
        var categoryBreakdown = await CalculateCategoryBreakdown(products, orders, cancellationToken);

        // Calculate monthly trends
        var monthlyTrends = CalculateMonthlyTrends(orders);

        return new GetProductAnalyticsResponse(
            overview,
            topSellingProducts,
            lowStockProducts,
            categoryBreakdown,
            monthlyTrends);
    }

    private static ProductOverviewAnalytics CalculateOverviewAnalytics(IEnumerable<Domain.Entities.Product> products)
    {
        var productList = products.ToList();
        var totalProducts = productList.Count;
        var activeProducts = productList.Count(p => p.IsActive);
        var inactiveProducts = totalProducts - activeProducts;
        var outOfStockProducts = productList.Count(p => p.StockQuantity == 0);
        var lowStockProducts = productList.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 10);
        var totalInventoryValue = productList.Sum(p => p.Price.Amount * p.StockQuantity);
        var averageProductPrice = productList.Any() ? productList.Average(p => p.Price.Amount) : 0;

        return new ProductOverviewAnalytics(
            totalProducts,
            activeProducts,
            inactiveProducts,
            outOfStockProducts,
            lowStockProducts,
            totalInventoryValue,
            averageProductPrice);
    }

    private static IEnumerable<ProductPerformanceAnalytics> CalculateTopSellingProducts(
        IEnumerable<Domain.Entities.Product> products, 
        IEnumerable<Domain.Entities.Order> orders)
    {
        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(oi => oi.ProductId)
            .ToDictionary(g => g.Key, g => new
            {
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.UnitPrice.Amount * oi.Quantity),
                LastSold = g.Max(oi => oi.Order.CreatedAt)
            });

        return products
            .Select(p => new ProductPerformanceAnalytics(
                p.Id,
                p.NameEn,
                p.NameAr,
                p.ProductSku,
                p.StockQuantity,
                productSales.ContainsKey(p.Id) ? productSales[p.Id].TotalSold : 0,
                productSales.ContainsKey(p.Id) ? productSales[p.Id].Revenue : 0,
                p.Price.Amount,
                p.Price.Currency,
                productSales.ContainsKey(p.Id) ? productSales[p.Id].LastSold : DateTime.MinValue))
            .OrderByDescending(p => p.TotalSold)
            .Take(10);
    }

    private static IEnumerable<ProductPerformanceAnalytics> CalculateLowStockProducts(IEnumerable<Domain.Entities.Product> products)
    {
        return products
            .Where(p => p.StockQuantity <= 10 && p.IsActive)
            .Select(p => new ProductPerformanceAnalytics(
                p.Id,
                p.NameEn,
                p.NameAr,
                p.ProductSku,
                p.StockQuantity,
                0, // We don't need sales data for low stock report
                0,
                p.Price.Amount,
                p.Price.Currency,
                DateTime.MinValue))
            .OrderBy(p => p.StockQuantity);
    }

    private async Task<IEnumerable<CategoryAnalytics>> CalculateCategoryBreakdown(
        IEnumerable<Domain.Entities.Product> products,
        IEnumerable<Domain.Entities.Order> orders,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(oi => oi.ProductId)
            .ToDictionary(g => g.Key, g => new
            {
                TotalSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.UnitPrice.Amount * oi.Quantity)
            });

        return categories.Select(c =>
        {
            var categoryProducts = products.Where(p => p.CategoryId == c.Id).ToList();
            var totalSold = categoryProducts.Sum(p => productSales.ContainsKey(p.Id) ? productSales[p.Id].TotalSold : 0);
            var revenue = categoryProducts.Sum(p => productSales.ContainsKey(p.Id) ? productSales[p.Id].Revenue : 0);
            var averagePrice = categoryProducts.Any() ? categoryProducts.Average(p => p.Price.Amount) : 0;

            return new CategoryAnalytics(
                c.Id,
                c.NameEn,
                c.NameAr,
                categoryProducts.Count,
                totalSold,
                revenue,
                averagePrice);
        });
    }

    private static IEnumerable<MonthlyProductAnalytics> CalculateMonthlyTrends(IEnumerable<Domain.Entities.Order> orders)
    {
        return orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyProductAnalytics(
                g.Key.Year,
                g.Key.Month,
                g.Sum(o => o.Items.Sum(oi => oi.Quantity)),
                g.Sum(o => o.TotalAmount.Amount),
                g.Count()))
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month);
    }
}