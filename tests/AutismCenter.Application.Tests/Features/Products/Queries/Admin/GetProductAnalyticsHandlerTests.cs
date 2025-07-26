using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Tests.Features.Products.Queries.Admin;

public class GetProductAnalyticsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetProductAnalyticsHandler _handler;

    public GetProductAnalyticsHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new GetProductAnalyticsHandler(
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAnalytics()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var product = Product.Create(
            "Test Product", "منتج اختبار", "Description", "وصف",
            Money.Create(100m, "USD"), 50, categoryId, "PRD-001");

        var query = new GetProductAnalyticsQuery();

        _productRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product });

        _orderRepositoryMock
            .Setup(x => x.GetOrdersWithItemsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { category });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Overview);
        Assert.Equal(1, result.Overview.TotalProducts);
        Assert.Equal(1, result.Overview.ActiveProducts);
        Assert.Equal(0, result.Overview.InactiveProducts);
        Assert.Equal(5000m, result.Overview.TotalInventoryValue); // 50 * 100
        Assert.Equal(100m, result.Overview.AverageProductPrice);

        Assert.NotNull(result.TopSellingProducts);
        Assert.NotNull(result.LowStockProducts);
        Assert.NotNull(result.CategoryBreakdown);
        Assert.NotNull(result.MonthlyTrends);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_FiltersProducts()
    {
        // Arrange
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        
        var product1 = Product.Create(
            "Product 1", "منتج 1", "Description 1", "وصف 1",
            Money.Create(100m, "USD"), 50, categoryId1, "PRD-001");
        var product2 = Product.Create(
            "Product 2", "منتج 2", "Description 2", "وصف 2",
            Money.Create(200m, "USD"), 30, categoryId2, "PRD-002");

        var query = new GetProductAnalyticsQuery(CategoryId: categoryId1);

        _productRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product1, product2 });

        _orderRepositoryMock
            .Setup(x => x.GetOrdersWithItemsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Overview.TotalProducts);
        Assert.Equal(5000m, result.Overview.TotalInventoryValue); // Only product1: 50 * 100
    }

    [Fact]
    public async Task Handle_WithLowStockProducts_IdentifiesLowStock()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var lowStockProduct = Product.Create(
            "Low Stock Product", "منتج مخزون قليل", "Description", "وصف",
            Money.Create(100m, "USD"), 5, categoryId, "PRD-001"); // 5 is below threshold of 10

        var normalStockProduct = Product.Create(
            "Normal Stock Product", "منتج مخزون عادي", "Description", "وصف",
            Money.Create(100m, "USD"), 50, categoryId, "PRD-002");

        var query = new GetProductAnalyticsQuery();

        _productRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { lowStockProduct, normalStockProduct });

        _orderRepositoryMock
            .Setup(x => x.GetOrdersWithItemsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Overview.LowStockProducts);
        Assert.Single(result.LowStockProducts);
        
        var lowStockResult = result.LowStockProducts.First();
        Assert.Equal("PRD-001", lowStockResult.ProductSku);
        Assert.Equal(5, lowStockResult.StockQuantity);
    }
}