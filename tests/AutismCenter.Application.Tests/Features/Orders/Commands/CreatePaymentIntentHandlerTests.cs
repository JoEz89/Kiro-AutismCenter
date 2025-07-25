using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Commands.CreatePaymentIntent;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class CreatePaymentIntentHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ILogger<CreatePaymentIntentHandler>> _loggerMock;
    private readonly CreatePaymentIntentHandler _handler;

    public CreatePaymentIntentHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _loggerMock = new Mock<ILogger<CreatePaymentIntentHandler>>();

        _handler = new CreatePaymentIntentHandler(
            _orderRepositoryMock.Object,
            _paymentServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldCreatePaymentIntentSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";
        var paymentIntentId = "pi_test_123";

        var shippingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);
        
        // Add some items to make the order have a total amount
        var product = CreateTestProduct();
        order.AddItem(product, 2, Money.Create(25.00m, "BHD"));

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentIntentAsync(
                It.IsAny<Money>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntentId);

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.PaymentIntentId.Should().Be(paymentIntentId);
        result.ClientSecret.Should().Be($"{paymentIntentId}_secret");
        result.ErrorMessage.Should().BeNull();

        _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(
            It.Is<Money>(m => m.Amount == 50.00m && m.Currency == "BHD"),
            "bhd",
            It.Is<Dictionary<string, string>>(d => 
                d["order_id"] == order.Id.ToString() &&
                d["order_number"] == orderNumber &&
                d["user_id"] == userId.ToString()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentIntentId.Should().BeNull();
        result.ClientSecret.Should().BeNull();
        result.ErrorMessage.Should().Be("Order not found");

        _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(
            It.IsAny<Money>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderAlreadyPaid_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";
        var existingPaymentId = "pi_existing_123";

        var shippingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);
        
        // Mark payment as already completed
        order.MarkPaymentCompleted(existingPaymentId);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentIntentId.Should().BeNull();
        result.ClientSecret.Should().BeNull();
        result.ErrorMessage.Should().Be("Order has already been paid");

        _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(
            It.IsAny<Money>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PaymentServiceFailure_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";

        var shippingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);
        
        // Add some items to make the order have a total amount
        var product = CreateTestProduct();
        order.AddItem(product, 1, Money.Create(30.00m, "BHD"));

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentIntentAsync(
                It.IsAny<Money>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty); // Simulate failure

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentIntentId.Should().BeNull();
        result.ClientSecret.Should().BeNull();
        result.ErrorMessage.Should().Be("Failed to create payment intent");
    }

    [Fact]
    public async Task Handle_PaymentServiceException_ShouldReturnFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";

        var shippingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);
        
        // Add some items to make the order have a total amount
        var product = CreateTestProduct();
        order.AddItem(product, 1, Money.Create(15.00m, "BHD"));

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentIntentAsync(
                It.IsAny<Money>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Payment service unavailable"));

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PaymentIntentId.Should().BeNull();
        result.ClientSecret.Should().BeNull();
        result.ErrorMessage.Should().Be("An error occurred while creating payment intent");
    }

    [Fact]
    public async Task Handle_ValidOrderWithZeroAmount_ShouldStillCreatePaymentIntent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderNumber = "ORD-2024-123456";
        var paymentIntentId = "pi_test_zero_amount";

        var shippingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var billingAddress = Address.Create("123 Main St", "Test City", "Test State", "12345", "Test Country");
        var order = Order.Create(userId, shippingAddress, billingAddress, orderNumber);
        // Order with no items will have zero total

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentIntentAsync(
                It.IsAny<Money>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentIntentId);

        var command = new CreatePaymentIntentCommand(orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.PaymentIntentId.Should().Be(paymentIntentId);
        result.ClientSecret.Should().Be($"{paymentIntentId}_secret");

        _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(
            It.Is<Money>(m => m.Amount == 0m && m.Currency == "BHD"),
            "bhd",
            It.IsAny<Dictionary<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Product CreateTestProduct()
    {
        return Product.Create(
            "Test Product",
            "Test Product AR",
            "Test Description",
            "Test Description AR",
            Money.Create(25.00m, "BHD"),
            100,
            Guid.NewGuid(), // categoryId
            "PRD-TEST-001");
    }
}