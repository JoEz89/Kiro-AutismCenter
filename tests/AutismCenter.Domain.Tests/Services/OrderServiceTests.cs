using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using Moq;

namespace AutismCenter.Domain.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderService = new OrderService(_orderRepositoryMock.Object, _productRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidData_ShouldReturnOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var billingAddress = Address.Create("456 Oak St", "Manama", "Capital", "12345", "Bahrain");
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var items = new List<(Guid ProductId, int Quantity)> { (productId, 2) };

        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");

        _orderRepositoryMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-2024-001");

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var order = await _orderService.CreateOrderAsync(userId, shippingAddress, billingAddress, items);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(userId, order.UserId);
        Assert.Equal("ORD-2024-001", order.OrderNumber);
        Assert.Single(order.Items);
        Assert.Equal(2, order.Items.First().Quantity);
        Assert.Equal(Money.Create(200, "BHD"), order.TotalAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInactiveProduct_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var billingAddress = Address.Create("456 Oak St", "Manama", "Capital", "12345", "Bahrain");
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var items = new List<(Guid ProductId, int Quantity)> { (productId, 2) };

        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");
        product.Deactivate();

        _orderRepositoryMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-2024-001");

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _orderService.CreateOrderAsync(userId, shippingAddress, billingAddress, items));
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var billingAddress = Address.Create("456 Oak St", "Manama", "Capital", "12345", "Bahrain");
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var items = new List<(Guid ProductId, int Quantity)> { (productId, 15) }; // More than available

        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001"); // Only 10 in stock

        _orderRepositoryMock.Setup(x => x.GenerateOrderNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ORD-2024-001");

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _orderService.CreateOrderAsync(userId, shippingAddress, billingAddress, items));
    }

    [Fact]
    public async Task CanFulfillOrderAsync_WithAvailableProducts_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var billingAddress = Address.Create("456 Oak St", "Manama", "Capital", "12345", "Bahrain");
        var categoryId = Guid.NewGuid();

        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");

        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        order.AddItem(product, 2, product.Price);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var canFulfill = await _orderService.CanFulfillOrderAsync(order);

        // Assert
        Assert.True(canFulfill);
    }

    [Fact]
    public void CalculateOrderTotal_WithMultipleItems_ShouldReturnCorrectTotal()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", "منتج 1", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");
        var product2 = Product.Create("Product 2", "منتج 2", "Description", "وصف", 
            Money.Create(50, "BHD"), 5, categoryId, "PRD-002");

        var items = new List<(Product Product, int Quantity)>
        {
            (product1, 2), // 200 BHD
            (product2, 3)  // 150 BHD
        };

        // Act
        var total = _orderService.CalculateOrderTotal(items);

        // Assert
        Assert.Equal(Money.Create(350, "BHD"), total);
    }

    [Fact]
    public void CalculateOrderTotal_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", "منتج 1", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");
        var product2 = Product.Create("Product 2", "منتج 2", "Description", "وصف", 
            Money.Create(50, "USD"), 5, categoryId, "PRD-002");

        var items = new List<(Product Product, int Quantity)>
        {
            (product1, 2),
            (product2, 3)
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _orderService.CalculateOrderTotal(items));
    }

    [Fact]
    public async Task ReserveInventoryAsync_ShouldReduceProductStock()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddress = Address.Create("123 Main St", "Manama", "Capital", "12345", "Bahrain");
        var billingAddress = Address.Create("456 Oak St", "Manama", "Capital", "12345", "Bahrain");
        var categoryId = Guid.NewGuid();

        var product = Product.Create("Test Product", "منتج تجريبي", "Description", "وصف", 
            Money.Create(100, "BHD"), 10, categoryId, "PRD-001");

        var order = Order.Create(userId, shippingAddress, billingAddress, "ORD-2024-001");
        order.AddItem(product, 3, product.Price);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _orderService.ReserveInventoryAsync(order);

        // Assert
        Assert.Equal(7, product.StockQuantity); // 10 - 3 = 7
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }
}