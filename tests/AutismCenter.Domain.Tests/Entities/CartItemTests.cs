using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Domain.Tests.Entities;

public class CartItemTests
{
    private readonly Guid _cartId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Money _unitPrice = Money.Create(15.75m, "BHD");

    [Fact]
    public void Create_ShouldCreateCartItemWithCorrectProperties()
    {
        // Arrange
        var quantity = 3;

        // Act
        var cartItem = CartItem.Create(_cartId, _productId, quantity, _unitPrice);

        // Assert
        Assert.Equal(_cartId, cartItem.CartId);
        Assert.Equal(_productId, cartItem.ProductId);
        Assert.Equal(quantity, cartItem.Quantity);
        Assert.Equal(_unitPrice, cartItem.UnitPrice);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CartItem.Create(_cartId, _productId, 0, _unitPrice));
    }

    [Fact]
    public void Create_WithNegativeQuantity_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CartItem.Create(_cartId, _productId, -1, _unitPrice));
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantityAndTimestamp()
    {
        // Arrange
        var cartItem = CartItem.Create(_cartId, _productId, 2, _unitPrice);
        var originalUpdateTime = cartItem.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        cartItem.UpdateQuantity(5);

        // Assert
        Assert.Equal(5, cartItem.Quantity);
        Assert.True(cartItem.UpdatedAt > originalUpdateTime);
    }

    [Fact]
    public void UpdateQuantity_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var cartItem = CartItem.Create(_cartId, _productId, 2, _unitPrice);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cartItem.UpdateQuantity(0));
    }

    [Fact]
    public void UpdateQuantity_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var cartItem = CartItem.Create(_cartId, _productId, 2, _unitPrice);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cartItem.UpdateQuantity(-1));
    }

    [Fact]
    public void UpdateUnitPrice_ShouldUpdatePriceAndTimestamp()
    {
        // Arrange
        var cartItem = CartItem.Create(_cartId, _productId, 2, _unitPrice);
        var newPrice = Money.Create(20.00m, "BHD");
        var originalUpdateTime = cartItem.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        cartItem.UpdateUnitPrice(newPrice);

        // Assert
        Assert.Equal(newPrice, cartItem.UnitPrice);
        Assert.True(cartItem.UpdatedAt > originalUpdateTime);
    }

    [Fact]
    public void GetTotalPrice_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var quantity = 4;
        var unitPrice = Money.Create(12.50m, "BHD");
        var cartItem = CartItem.Create(_cartId, _productId, quantity, unitPrice);

        // Act
        var totalPrice = cartItem.GetTotalPrice();

        // Assert
        Assert.Equal(50.00m, totalPrice.Amount); // 4 * 12.50
        Assert.Equal("BHD", totalPrice.Currency);
    }

    [Fact]
    public void GetTotalPrice_WithSingleQuantity_ShouldReturnUnitPrice()
    {
        // Arrange
        var cartItem = CartItem.Create(_cartId, _productId, 1, _unitPrice);

        // Act
        var totalPrice = cartItem.GetTotalPrice();

        // Assert
        Assert.Equal(_unitPrice.Amount, totalPrice.Amount);
        Assert.Equal(_unitPrice.Currency, totalPrice.Currency);
    }
}