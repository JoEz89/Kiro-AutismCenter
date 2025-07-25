using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Orders.Commands.RefundPayment;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class RefundPaymentHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RefundPaymentHandler>> _loggerMock;
    private readonly RefundPaymentHandler _handler;

    public RefundPaymentHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RefundPaymentHandler>>();

        _handler = new RefundPaymentHandler(
            _orderRepositoryMock.Object,
            _paymentServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldProcessRefundSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentId = "pi_test_123";
        var refundId = "re_test_123";
        var refundAmount = Money.Create(50.00m, "BHD");

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");
        order.MarkPaymentCompleted(paymentId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.ProcessRefundAsync(paymentId, It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefundResult(true, refundId, null, refundAmount));

        var command = new RefundPaymentCommand(orderId, refundAmount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.RefundId.Should().Be(refundId);
        result.ErrorMessage.Should().BeNull();
        result.RefundedAmount.Should().Be(refundAmount);

        order.Status.Should().Be(OrderStatus.Refunded);
        order.PaymentStatus.Should().Be(Domain.Enums.PaymentStatus.Refunded);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new RefundPaymentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.RefundId.Should().BeNull();
        result.ErrorMessage.Should().Be("Order not found");

        _paymentServiceMock.Verify(x => x.ProcessRefundAsync(It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentNotCompleted_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");
        // Order payment status remains Pending

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new RefundPaymentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.RefundId.Should().BeNull();
        result.ErrorMessage.Should().Be("Cannot refund order that hasn't been paid");

        _paymentServiceMock.Verify(x => x.ProcessRefundAsync(It.IsAny<string>(), It.IsAny<Money>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RefundServiceFailure_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentId = "pi_test_123";
        var errorMessage = "Refund failed";

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");
        order.MarkPaymentCompleted(paymentId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.ProcessRefundAsync(paymentId, It.IsAny<Money>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefundResult(false, null, errorMessage, Money.Create(0)));

        var command = new RefundPaymentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.RefundId.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        order.Status.Should().Be(OrderStatus.Pending); // Should remain unchanged
        order.PaymentStatus.Should().Be(Domain.Enums.PaymentStatus.Completed); // Should remain unchanged

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}