using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Commands.UpdateOrderStatus;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateOrderStatusHandler(_orderRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ConfirmPendingOrder_ShouldConfirmOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Confirmed);
        order.Status.Should().Be(OrderStatus.Confirmed);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StartProcessingConfirmedOrder_ShouldStartProcessingSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));
        order.Confirm();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Processing);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Processing);
        order.Status.Should().Be(OrderStatus.Processing);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShipProcessingOrder_ShouldShipOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Shipped);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Shipped);
        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeliverShippedOrder_ShouldDeliverOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));
        order.Confirm();
        order.StartProcessing();
        order.Ship();

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Delivered);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Delivered);
        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancelOrder_ShouldCancelOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Cancelled);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RefundOrder_ShouldRefundOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));
        order.MarkPaymentCompleted("payment_123456");

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Refunded);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Refunded);
        result.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        order.Status.Should().Be(OrderStatus.Refunded);
        order.PaymentStatus.Should().Be(PaymentStatus.Refunded);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Confirmed);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order with ID {orderId} not found");
    }

    [Fact]
    public async Task Handle_InvalidStatusTransition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        // Order is pending, trying to ship directly should fail

        var command = new UpdateOrderStatusCommand(orderId, OrderStatus.Shipped);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    public async Task Handle_InvalidStatusValue_ShouldThrowInvalidOperationException(OrderStatus invalidStatus)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");

        var command = new UpdateOrderStatusCommand(orderId, invalidStatus);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Invalid status transition to {invalidStatus}");
    }
}




