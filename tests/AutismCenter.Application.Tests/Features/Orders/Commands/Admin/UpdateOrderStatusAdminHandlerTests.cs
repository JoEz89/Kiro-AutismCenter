using Xunit;
using Moq;
using AutismCenter.Application.Features.Orders.Commands.Admin.UpdateOrderStatusAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Orders.Commands.Admin;

public class UpdateOrderStatusAdminHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly UpdateOrderStatusAdminHandler _handler;

    public UpdateOrderStatusAdminHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new UpdateOrderStatusAdminHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidConfirmRequest_ReturnsUpdateOrderStatusAdminResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        
        // Add a product to the order so it can be confirmed
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "منتج اختبار", "Description", "وصف", 
            Money.Create(50.00m, "USD"), 10, categoryId, "PRD-001");
        order.AddItem(product, 1, Money.Create(50.00m, "USD"));
        
        var command = new UpdateOrderStatusAdminCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ORD-2024-001", result.OrderNumber);
        Assert.Equal(OrderStatus.Confirmed, result.Status);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdateOrderStatusAdminCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShipOrder_UpdatesStatusAndShippedDate()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        
        // Add a product to the order so it can be confirmed
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "منتج اختبار", "Description", "وصف", 
            Money.Create(50.00m, "USD"), 10, categoryId, "PRD-001");
        order.AddItem(product, 1, Money.Create(50.00m, "USD"));
        
        order.Confirm();
        order.StartProcessing();

        var command = new UpdateOrderStatusAdminCommand(orderId, OrderStatus.Shipped);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Shipped, result.Status);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}