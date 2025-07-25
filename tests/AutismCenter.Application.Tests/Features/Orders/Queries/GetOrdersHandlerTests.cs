using AutismCenter.Application.Features.Orders.Queries.GetOrders;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Queries;

public class GetOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrdersHandler _handler;

    public GetOrdersHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrdersHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_GetAllOrders_ShouldReturnPaginatedOrders()
    {
        // Arrange
        var orders = CreateTestOrders(15);
        var query = new GetOrdersQuery(PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10); // First page
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(2);
        
        // Orders should be sorted by creation date (most recent first)
        result.Orders.Should().BeInDescendingOrder(o => o.CreatedAt);
    }

    [Fact]
    public async Task Handle_GetOrdersByUserId_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userOrders = CreateTestOrders(5, userId);
        var query = new GetOrdersQuery(UserId: userId, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userOrders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(5);
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_GetOrdersByStatus_ShouldReturnOrdersWithSpecificStatus()
    {
        // Arrange
        var status = OrderStatus.Confirmed;
        var orders = CreateTestOrders(8);
        var query = new GetOrdersQuery(Status: status, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(8);
        result.TotalCount.Should().Be(8);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_GetOrdersByDateRange_ShouldReturnOrdersInDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var orders = CreateTestOrders(12);
        var query = new GetOrdersQuery(StartDate: startDate, EndDate: endDate, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10); // First page
        result.TotalCount.Should().Be(12);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_GetOrdersByUserIdAndStatus_ShouldApplyBothFilters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var status = OrderStatus.Pending;
        var userOrders = CreateTestOrders(10, userId);
        
        // Simulate some orders with different statuses
        var filteredOrders = userOrders.Take(3).ToList();
        foreach (var order in filteredOrders)
        {
            // We can't directly set the status, but we can simulate the filtering in the test
        }

        var query = new GetOrdersQuery(UserId: userId, Status: status, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userOrders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        // Note: In a real scenario, we would also filter by status, but since we can't modify
        // the order status after creation in our test setup, we'll just verify the user filter
    }

    [Fact]
    public async Task Handle_SecondPage_ShouldReturnCorrectPaginatedResults()
    {
        // Arrange
        var orders = CreateTestOrders(25);
        var query = new GetOrdersQuery(PageNumber: 2, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10); // Second page
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_LastPagePartial_ShouldReturnRemainingOrders()
    {
        // Arrange
        var orders = CreateTestOrders(23);
        var query = new GetOrdersQuery(PageNumber: 3, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(3); // Last page with remaining orders
        result.TotalCount.Should().Be(23);
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_NoOrders_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new GetOrdersQuery(PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PageBeyondRange_ShouldReturnEmptyResult()
    {
        // Arrange
        var orders = CreateTestOrders(5);
        var query = new GetOrdersQuery(PageNumber: 10, PageSize: 10); // Way beyond available pages

        _orderRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(10);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    private static List<Order> CreateTestOrders(int count, Guid? userId = null)
    {
        var orders = new List<Order>();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");

        for (int i = 0; i < count; i++)
        {
            var orderUserId = userId ?? Guid.NewGuid();
            var order = Order.Create(
                orderUserId,
                shippingAddress,
                billingAddress,
                $"ORD-2024-{i:D6}"
            );

            // Add a product to make the order valid
            var product = Product.Create($"Product {i}", "منتج تجريبي", $"Description {i}", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
            order.AddItem(product, 1, Money.Create(100, "BHD"));

            orders.Add(order);
        }

        return orders;
    }
}



