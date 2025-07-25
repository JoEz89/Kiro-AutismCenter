using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AutismCenter.Domain.Tests.Entities;

public class OrderItemTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateOrderItemSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var quantity = 3;
        var unitPrice = Money.Create(100, "BHD");

        // Act
        var orderItem = OrderItem.Create(orderId, productId, quantity, unitPrice);

        // Assert
        orderItem.Should().NotBeNull();
        orderItem.OrderId.Should().Be(orderId);
        orderItem.ProductId.Should().Be(productId);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.UnitPrice.Should().Be(unitPrice);
    }

    [Fact]
    public void Create_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var unitPrice = Money.Create(100, "BHD");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(orderId, productId, 0, unitPrice));
    }

    [Fact]
    public void Create_NegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var unitPrice = Money.Create(100, "BHD");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(orderId, productId, -1, unitPrice));
    }

    [Fact]
    public void UpdateQuantity_ValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, Money.Create(100, "BHD"));
        var newQuantity = 5;

        // Act
        orderItem.UpdateQuantity(newQuantity);

        // Assert
        orderItem.Quantity.Should().Be(newQuantity);
    }

    [Fact]
    public void UpdateQuantity_ZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, Money.Create(100, "BHD"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => orderItem.UpdateQuantity(0));
    }

    [Fact]
    public void UpdateQuantity_NegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 2, Money.Create(100, "BHD"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => orderItem.UpdateQuantity(-1));
    }

    [Fact]
    public void GetTotalPrice_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var unitPrice = Money.Create(100, "BHD");
        var quantity = 3;
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), quantity, unitPrice);

        // Act
        var totalPrice = orderItem.GetTotalPrice();

        // Assert
        totalPrice.Amount.Should().Be(300);
        totalPrice.Currency.Should().Be("BHD");
    }

    [Fact]
    public void GetTotalPrice_SingleItem_ShouldReturnUnitPrice()
    {
        // Arrange
        var unitPrice = Money.Create(150, "BHD");
        var orderItem = OrderItem.Create(Guid.NewGuid(), Guid.NewGuid(), 1, unitPrice);

        // Act
        var totalPrice = orderItem.GetTotalPrice();

        // Assert
        totalPrice.Should().Be(unitPrice);
    }
}