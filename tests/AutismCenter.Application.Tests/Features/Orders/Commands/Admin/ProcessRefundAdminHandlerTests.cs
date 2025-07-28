using Xunit;
using Moq;
using AutismCenter.Application.Features.Orders.Commands.Admin.ProcessRefundAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Orders.Commands.Admin;

public class ProcessRefundAdminHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly ProcessRefundAdminHandler _handler;

    public ProcessRefundAdminHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new ProcessRefundAdminHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefundRequest_ReturnsProcessRefundAdminResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        
        // Add a product to the order so it has a total amount
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "منتج اختبار", "Description", "وصف", 
            Money.Create(100.00m, "USD"), 10, categoryId, "PRD-001");
        order.AddItem(product, 1, Money.Create(100.00m, "USD"));
        
        order.MarkPaymentCompleted("payment_123");

        var command = new ProcessRefundAdminCommand(orderId, 50.00m, "Customer request");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ORD-2024-001", result.OrderNumber);
        Assert.Equal(50.00m, result.RefundAmount);
        Assert.Equal(OrderStatus.Refunded, result.Status);
        Assert.Equal(PaymentStatus.Refunded, result.PaymentStatus);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ProcessRefundAdminCommand(orderId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidRefundAmount_ThrowsValidationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        
        // Add a product to the order so it has a total amount
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "منتج اختبار", "Description", "وصف", 
            Money.Create(100.00m, "USD"), 10, categoryId, "PRD-001");
        order.AddItem(product, 1, Money.Create(100.00m, "USD"));
        
        order.MarkPaymentCompleted("payment_123");

        var command = new ProcessRefundAdminCommand(orderId, 150.00m); // More than order total

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}