using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Orders.Commands.CancelOrder;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Commands;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CancelOrderHandler(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPendingOrder_ShouldCancelOrderAndRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 5, Guid.NewGuid(), "PRD-001"); // Initial stock
        order.AddItem(product, 2, Money.Create(100, "BHD"));

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidConfirmedOrder_ShouldCancelOrderAndRestoreStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        product.UpdateStock(5);
        order.AddItem(product, 2, Money.Create(100, "BHD"));
        order.Confirm();

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminCancellation_ShouldCancelOrderWithoutUserValidation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        product.UpdateStock(5);
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var command = new CancelOrderCommand(orderId); // No UserId provided (admin cancellation)

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleItems_ShouldRestoreStockForAllItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        
        var product1 = Product.Create("Product 1", "منتج تجريبي", "Description 1", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        var product2 = Product.Create("Product 2", "منتج تجريبي", "Description 2", "وصف تجريبي", Money.Create(50, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        
        product1.UpdateStock(10);
        product2.UpdateStock(5);
        
        order.AddItem(product1, 2, Money.Create(100, "BHD"));
        order.AddItem(product2, 1, Money.Create(50, "BHD"));

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(product2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);

        _productRepositoryMock.Verify(x => x.UpdateAsync(product1, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product2, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldContinueWithOtherProducts()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 2, Money.Create(100, "BHD"));

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null); // Product not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        order.Status.Should().Be(OrderStatus.Cancelled);

        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new CancelOrderCommand(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order with ID {orderId} not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderUserId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid(); // Different user
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(orderUserId, shippingAddress, billingAddress, "ORD-2024-123456");

        var command = new CancelOrderCommand(orderId, requestingUserId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to cancel this order");
    }

    [Fact]
    public async Task Handle_DeliveredOrder_ShouldThrowInvalidOperationException()
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
        order.Deliver(); // Order is delivered, cannot be cancelled

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order with status {OrderStatus.Delivered} cannot be cancelled");
    }

    [Fact]
    public async Task Handle_AlreadyCancelledOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));
        order.Cancel(); // Already cancelled

        var command = new CancelOrderCommand(orderId, userId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Order with status {OrderStatus.Cancelled} cannot be cancelled");
    }
}




