using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Common.Models;
using AutismCenter.Application.Features.Orders.Commands.ProcessPayment;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class ProcessPaymentHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProcessPaymentHandler>> _loggerMock;
    private readonly ProcessPaymentHandler _handler;

    public ProcessPaymentHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProcessPaymentHandler>>();

        _handler = new ProcessPaymentHandler(
            _orderRepositoryMock.Object,
            _paymentServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldProcessPaymentSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";
        var paymentId = "pi_test_123";

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, paymentId, null, Application.Common.Models.PaymentStatus.Succeeded));

        var command = new ProcessPaymentCommand(orderId, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.PaymentId.Should().Be(paymentId);
        result.ErrorMessage.Should().BeNull();

        order.PaymentStatus.Should().Be(Domain.Enums.PaymentStatus.Completed);
        order.PaymentId.Should().Be(paymentId);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new ProcessPaymentCommand(orderId, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentId.Should().BeNull();
        result.ErrorMessage.Should().Be("Order not found");

        _paymentServiceMock.Verify(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";
        var existingPaymentId = "pi_existing_123";

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");
        order.MarkPaymentCompleted(existingPaymentId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new ProcessPaymentCommand(orderId, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentId.Should().Be(existingPaymentId);
        result.ErrorMessage.Should().Be("Payment already completed");

        _paymentServiceMock.Verify(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentServiceFailure_ShouldMarkPaymentFailedAndReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentMethodId = "pm_test_123";
        var errorMessage = "Payment declined";

        var shippingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "12345", "Test Country");
        var order = Order.Create(Guid.NewGuid(), shippingAddress, billingAddress, "ORD-2024-123456");

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, null, errorMessage, Application.Common.Models.PaymentStatus.Failed));

        var command = new ProcessPaymentCommand(orderId, paymentMethodId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentId.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        order.PaymentStatus.Should().Be(Domain.Enums.PaymentStatus.Failed);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}