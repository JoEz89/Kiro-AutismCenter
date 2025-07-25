using AutismCenter.Application.Features.Orders.Queries.GetOrderById;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Orders.Queries;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 2, Money.Create(100, "BHD"));

        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.OrderNumber.Should().Be("ORD-2024-123456");
        result.UserId.Should().Be(userId);
        result.TotalAmount.Should().Be(200);
        result.Currency.Should().Be("BHD");
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(product.Id);
        result.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ExistingOrderWithUserIdValidation_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var query = new GetOrderByIdQuery(orderId, userId); // Include UserId for validation

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_NonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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

        var query = new GetOrderByIdQuery(orderId, requestingUserId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to view this order");
    }

    [Fact]
    public async Task Handle_AdminAccess_ShouldReturnOrderWithoutUserValidation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderUserId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(orderUserId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var query = new GetOrderByIdQuery(orderId); // No UserId provided (admin access)

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.UserId.Should().Be(orderUserId);
    }

    [Fact]
    public async Task Handle_OrderWithMultipleItems_ShouldReturnAllItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "City", "State", "12345", "Country");
        var billingAddress = Address.Create("456 Billing St", "City", "State", "54321", "Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        
        var product1 = Product.Create("Product 1", "منتج تجريبي", "Description 1", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        var product2 = Product.Create("Product 2", "منتج تجريبي", "Description 2", "وصف تجريبي", Money.Create(50, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        
        order.AddItem(product1, 2, Money.Create(100, "BHD"));
        order.AddItem(product2, 3, Money.Create(50, "BHD"));

        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(350); // (2 * 100) + (3 * 50)
        
        var item1 = result.Items.First(i => i.ProductId == product1.Id);
        item1.Quantity.Should().Be(2);
        item1.UnitPrice.Should().Be(100);
        item1.TotalPrice.Should().Be(200);
        
        var item2 = result.Items.First(i => i.ProductId == product2.Id);
        item2.Quantity.Should().Be(3);
        item2.UnitPrice.Should().Be(50);
        item2.TotalPrice.Should().Be(150);
    }

    [Fact]
    public async Task Handle_OrderWithAddresses_ShouldReturnCorrectAddresses()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Shipping St", "Shipping City", "Shipping State", "12345", "Shipping Country");
        var billingAddress = Address.Create("456 Billing Ave", "Billing City", "Billing State", "54321", "Billing Country");
        
        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-123456");
        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف تجريبي", Money.Create(100, "BHD"), 10, Guid.NewGuid(), "PRD-001");
        order.AddItem(product, 1, Money.Create(100, "BHD"));

        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        result!.ShippingAddress.Street.Should().Be("123 Shipping St");
        result.ShippingAddress.City.Should().Be("Shipping City");
        result.ShippingAddress.State.Should().Be("Shipping State");
        result.ShippingAddress.PostalCode.Should().Be("12345");
        result.ShippingAddress.Country.Should().Be("Shipping Country");
        
        result.BillingAddress.Street.Should().Be("456 Billing Ave");
        result.BillingAddress.City.Should().Be("Billing City");
        result.BillingAddress.State.Should().Be("Billing State");
        result.BillingAddress.PostalCode.Should().Be("54321");
        result.BillingAddress.Country.Should().Be("Billing Country");
    }
}




