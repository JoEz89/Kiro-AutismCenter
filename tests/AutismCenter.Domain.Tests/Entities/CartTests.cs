using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;
using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Tests.Entities;

public class CartTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Money _unitPrice = Money.Create(10.50m, "BHD");

    [Fact]
    public void Create_ShouldCreateCartWithCorrectProperties()
    {
        // Act
        var cart = Cart.Create(_userId);

        // Assert
        Assert.Equal(_userId, cart.UserId);
        Assert.True(cart.ExpiresAt > DateTime.UtcNow);
        Assert.Empty(cart.Items);
        Assert.True(cart.IsEmpty());
        Assert.Equal(0, cart.GetTotalItemCount());
        Assert.Single(cart.DomainEvents);
        Assert.IsType<CartCreatedEvent>(cart.DomainEvents.First());
    }

    [Fact]
    public void Create_WithCustomExpiration_ShouldSetCorrectExpirationDate()
    {
        // Arrange
        var customExpiration = DateTime.UtcNow.AddDays(7);

        // Act
        var cart = Cart.Create(_userId, customExpiration);

        // Assert
        Assert.Equal(customExpiration, cart.ExpiresAt);
    }

    [Fact]
    public void AddItem_ShouldAddNewItemToCart()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var quantity = 2;

        // Act
        cart.AddItem(_productId, quantity, _unitPrice);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(quantity, cart.GetTotalItemCount());
        Assert.Equal(_productId, cart.Items.First().ProductId);
        Assert.Equal(quantity, cart.Items.First().Quantity);
        Assert.Equal(_unitPrice, cart.Items.First().UnitPrice);
        Assert.Contains(cart.DomainEvents, e => e is CartItemAddedEvent);
    }

    [Fact]
    public void AddItem_ExistingProduct_ShouldIncreaseQuantity()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);
        cart.ClearDomainEvents();

        // Act
        cart.AddItem(_productId, 3, _unitPrice);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(5, cart.Items.First().Quantity);
        Assert.Equal(5, cart.GetTotalItemCount());
        Assert.Contains(cart.DomainEvents, e => e is CartItemAddedEvent);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(_productId, 0, _unitPrice));
    }

    [Fact]
    public void AddItem_ToExpiredCart_ShouldThrowException()
    {
        // Arrange
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        var cart = Cart.Create(_userId, expiredDate);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.AddItem(_productId, 1, _unitPrice));
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateExistingItem()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);
        cart.ClearDomainEvents();

        // Act
        cart.UpdateItemQuantity(_productId, 5);

        // Assert
        Assert.Equal(5, cart.Items.First().Quantity);
        Assert.Equal(5, cart.GetTotalItemCount());
        Assert.Contains(cart.DomainEvents, e => e is CartItemQuantityUpdatedEvent);
    }

    [Fact]
    public void UpdateItemQuantity_WithZeroQuantity_ShouldRemoveItem()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);
        cart.ClearDomainEvents();

        // Act
        cart.UpdateItemQuantity(_productId, 0);

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.GetTotalItemCount());
        Assert.Contains(cart.DomainEvents, e => e is CartItemRemovedEvent);
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentProduct_ShouldThrowException()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var nonExistentProductId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.UpdateItemQuantity(nonExistentProductId, 1));
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromCart()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);
        cart.ClearDomainEvents();

        // Act
        cart.RemoveItem(_productId);

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.GetTotalItemCount());
        Assert.Contains(cart.DomainEvents, e => e is CartItemRemovedEvent);
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_ShouldNotThrowException()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var nonExistentProductId = Guid.NewGuid();

        // Act & Assert (should not throw)
        cart.RemoveItem(nonExistentProductId);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);
        cart.AddItem(Guid.NewGuid(), 3, _unitPrice);
        cart.ClearDomainEvents();

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.GetTotalItemCount());
        Assert.True(cart.IsEmpty());
        Assert.Contains(cart.DomainEvents, e => e is CartClearedEvent);
    }

    [Fact]
    public void Clear_EmptyCart_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.ClearDomainEvents();

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.DomainEvents);
    }

    [Fact]
    public void GetTotalAmount_ShouldCalculateCorrectTotal()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var product1Price = Money.Create(10.50m, "BHD");
        var product2Price = Money.Create(25.00m, "BHD");
        
        cart.AddItem(_productId, 2, product1Price); // 2 * 10.50 = 21.00
        cart.AddItem(Guid.NewGuid(), 1, product2Price); // 1 * 25.00 = 25.00

        // Act
        var total = cart.GetTotalAmount();

        // Assert
        Assert.Equal(46.00m, total.Amount); // 21.00 + 25.00
        Assert.Equal("BHD", total.Currency);
    }

    [Fact]
    public void GetTotalAmount_EmptyCart_ShouldReturnZero()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act
        var total = cart.GetTotalAmount();

        // Assert
        Assert.Equal(0m, total.Amount);
    }

    [Fact]
    public void IsExpired_WithExpiredDate_ShouldReturnTrue()
    {
        // Arrange
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        var cart = Cart.Create(_userId, expiredDate);

        // Act & Assert
        Assert.True(cart.IsExpired());
    }

    [Fact]
    public void IsExpired_WithFutureDate_ShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var cart = Cart.Create(_userId, futureDate);

        // Act & Assert
        Assert.False(cart.IsExpired());
    }

    [Fact]
    public void ExtendExpiration_ShouldUpdateExpirationDate()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var originalExpiration = cart.ExpiresAt;

        // Act
        cart.ExtendExpiration(60); // 60 days

        // Assert
        Assert.True(cart.ExpiresAt > originalExpiration);
        Assert.True(cart.ExpiresAt > DateTime.UtcNow.AddDays(59));
    }

    [Fact]
    public void HasItem_ExistingProduct_ShouldReturnTrue()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 1, _unitPrice);

        // Act & Assert
        Assert.True(cart.HasItem(_productId));
    }

    [Fact]
    public void HasItem_NonExistentProduct_ShouldReturnFalse()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var nonExistentProductId = Guid.NewGuid();

        // Act & Assert
        Assert.False(cart.HasItem(nonExistentProductId));
    }

    [Fact]
    public void GetItem_ExistingProduct_ShouldReturnCartItem()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);

        // Act
        var item = cart.GetItem(_productId);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(_productId, item.ProductId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void GetItem_NonExistentProduct_ShouldReturnNull()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var item = cart.GetItem(nonExistentProductId);

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public void ValidateStock_WithSufficientStock_ShouldNotThrow()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 2, _unitPrice);

        var product = Product.Create("Test", "تست", "Description", "وصف", _unitPrice, 10, Guid.NewGuid(), "SKU001");
        // Use reflection to set the product ID to match the cart item
        var idField = typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(product, _productId);
        var products = new[] { product };

        // Act & Assert (should not throw)
        cart.ValidateStock(products);
    }

    [Fact]
    public void ValidateStock_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 5, _unitPrice);

        var product = Product.Create("Test", "تست", "Description", "وصف", _unitPrice, 3, Guid.NewGuid(), "SKU001");
        // Use reflection to set the product ID to match the cart item
        var idField = typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(product, _productId);
        var products = new[] { product };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.ValidateStock(products));
    }

    [Fact]
    public void ValidateStock_WithInactiveProduct_ShouldThrowException()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_productId, 1, _unitPrice);

        var product = Product.Create("Test", "تست", "Description", "وصف", _unitPrice, 10, Guid.NewGuid(), "SKU001");
        // Use reflection to set the product ID to match the cart item
        var idField = typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(product, _productId);
        product.Deactivate();
        var products = new[] { product };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.ValidateStock(products));
    }
}