using AutismCenter.Application.Features.Orders.Queries.GetOrderHistory;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Queries;

public class GetOrderHistoryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderHistoryHandler _handler;

    public GetOrderHistoryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderHistoryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UserWithOrders_ShouldReturnPaginatedOrderHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(15, userId);
        var query = new GetOrderHistoryQuery(userId, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10); // First page
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(2);
        
        // Orders should be sorted by creation date (most recent first)
        result.Orders.Should().BeInDescendingOrder(o => o.CreatedAt);
    }

    [Fact]
    public async Task Handle_SecondPage_ShouldReturnCorrectPaginatedResults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(25, userId);
        var query = new GetOrderHistoryQuery(userId, PageNumber: 2, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10); // Second page
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_LastPagePartial_ShouldReturnRemainingOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(23, userId);
        var query = new GetOrderHistoryQuery(userId, PageNumber: 3, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(3); // Last page with remaining orders
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        result.TotalCount.Should().Be(23);
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_UserWithNoOrders_ShouldReturnEmptyResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetOrderHistoryQuery(userId, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
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
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(5, userId);
        var query = new GetOrderHistoryQuery(userId, PageNumber: 10, PageSize: 10); // Way beyond available pages

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task Handle_CustomPageSize_ShouldRespectPageSize()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(20, userId);
        var query = new GetOrderHistoryQuery(userId, PageNumber: 1, PageSize: 5);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(5); // Custom page size
        result.TotalCount.Should().Be(20);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(4); // 20 orders / 5 per page = 4 pages
    }

    [Fact]
    public async Task Handle_OrdersWithDifferentStatuses_ShouldReturnAllUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(10, userId);
        
        // Simulate different order statuses by confirming some orders
        var confirmedOrders = orders.Take(3).ToList();
        foreach (var order in confirmedOrders)
        {
                var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
            order.AddItem(product, 1, Money.Create(100, "BHD"));
            order.Confirm();
        }

        var query = new GetOrderHistoryQuery(userId, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(10);
        result.Orders.Should().OnlyContain(o => o.UserId == userId);
        
        // Should include orders with different statuses
        result.Orders.Should().Contain(o => o.Status == Domain.Enums.OrderStatus.Pending);
        result.Orders.Should().Contain(o => o.Status == Domain.Enums.OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_OrdersWithItems_ShouldIncludeOrderItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(3, userId);
        
        // Add items to orders
        foreach (var order in orders)
        {
            var product = Product.Create($"Product for order {order.OrderNumber}", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
            order.AddItem(product, 2, Money.Create(100, "BHD"));
        }

        var query = new GetOrderHistoryQuery(userId, PageNumber: 1, PageSize: 10);

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(3);
        
        foreach (var orderDto in result.Orders)
        {
            orderDto.Items.Should().HaveCount(1);
            orderDto.Items.First().Quantity.Should().Be(2);
            orderDto.TotalAmount.Should().Be(200); // 2 * 100
        }
    }

    [Fact]
    public async Task Handle_DefaultPagination_ShouldUseDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = CreateTestOrdersForUser(5, userId);
        var query = new GetOrderHistoryQuery(userId); // Using default pagination

        _orderRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Orders.Should().HaveCount(5);
        result.PageNumber.Should().Be(1); // Default page number
        result.PageSize.Should().Be(10); // Default page size
        result.TotalPages.Should().Be(1);
    }

    private static List<Order> CreateTestOrdersForUser(int count, Guid userId)
    {
        var orders = new List<Order>();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");

        for (int i = 0; i < count; i++)
        {
            var order = Order.Create(
                userId,
                shippingAddress,
                billingAddress,
                $"ORD-2024-{i:D6}"
            );

            orders.Add(order);
        }

        return orders;
    }
}



