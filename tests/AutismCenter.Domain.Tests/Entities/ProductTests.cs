using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;
using FluentAssertions;

namespace AutismCenter.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var nameEn = "Test Product";
        var nameAr = "منتج تجريبي";
        var descriptionEn = "Test Description";
        var descriptionAr = "وصف تجريبي";
        var price = Money.Create(10.50m, "BHD");
        var stockQuantity = 100;
        var categoryId = Guid.NewGuid();
        var productSku = "PRD-001";

        // Act
        var product = Product.Create(nameEn, nameAr, descriptionEn, descriptionAr, price, stockQuantity, categoryId, productSku);

        // Assert
        product.Should().NotBeNull();
        product.NameEn.Should().Be(nameEn);
        product.NameAr.Should().Be(nameAr);
        product.DescriptionEn.Should().Be(descriptionEn);
        product.DescriptionAr.Should().Be(descriptionAr);
        product.Price.Should().Be(price);
        product.StockQuantity.Should().Be(stockQuantity);
        product.CategoryId.Should().Be(categoryId);
        product.ProductSku.Should().Be(productSku);
        product.IsActive.Should().BeTrue();
        product.GetDomainEvents().Should().ContainSingle(e => e is ProductCreatedEvent);
    }

    [Theory]
    [InlineData("", "منتج", "desc", "وصف", "English name cannot be empty")]
    [InlineData("Product", "", "desc", "وصف", "Arabic name cannot be empty")]
    [InlineData("Product", "منتج", "", "وصف", "English description cannot be empty")]
    [InlineData("Product", "منتج", "desc", "", "Arabic description cannot be empty")]
    public void Create_WithInvalidData_ShouldThrowArgumentException(string nameEn, string nameAr, string descEn, string descAr, string expectedMessage)
    {
        // Arrange
        var price = Money.Create(10.50m, "BHD");
        var stockQuantity = 100;
        var categoryId = Guid.NewGuid();
        var productSku = "PRD-001";

        // Act & Assert
        var act = () => Product.Create(nameEn, nameAr, descEn, descAr, price, stockQuantity, categoryId, productSku);
        act.Should().Throw<ArgumentException>()
           .WithMessage($"{expectedMessage}*");
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowArgumentException()
    {
        // Arrange
        var price = Money.Create(10.50m, "BHD");
        var stockQuantity = -1;
        var categoryId = Guid.NewGuid();

        // Act & Assert
        var act = () => Product.Create("Product", "منتج", "desc", "وصف", price, stockQuantity, categoryId, "PRD-001");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Stock quantity cannot be negative*");
    }

    [Fact]
    public void Create_WithEmptyProductSku_ShouldThrowArgumentException()
    {
        // Arrange
        var price = Money.Create(10.50m, "BHD");
        var stockQuantity = 100;
        var categoryId = Guid.NewGuid();

        // Act & Assert
        var act = () => Product.Create("Product", "منتج", "desc", "وصف", price, stockQuantity, categoryId, "");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Product SKU cannot be empty*");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        var product = CreateTestProduct();
        var newNameEn = "Updated Product";
        var newNameAr = "منتج محدث";
        var newDescEn = "Updated Description";
        var newDescAr = "وصف محدث";
        var newPrice = Money.Create(15.75m, "BHD");

        // Act
        product.UpdateDetails(newNameEn, newNameAr, newDescEn, newDescAr, newPrice);

        // Assert
        product.NameEn.Should().Be(newNameEn);
        product.NameAr.Should().Be(newNameAr);
        product.DescriptionEn.Should().Be(newDescEn);
        product.DescriptionAr.Should().Be(newDescAr);
        product.Price.Should().Be(newPrice);
        product.GetDomainEvents().Should().Contain(e => e is ProductUpdatedEvent);
    }

    [Fact]
    public void UpdateStock_WithValidQuantity_ShouldUpdateStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var newQuantity = 50;

        // Act
        product.UpdateStock(newQuantity);

        // Assert
        product.StockQuantity.Should().Be(newQuantity);
        product.GetDomainEvents().Should().Contain(e => e is ProductStockUpdatedEvent);
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act & Assert
        var act = () => product.UpdateStock(-1);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Stock quantity cannot be negative*");
    }

    [Fact]
    public void ReduceStock_WithValidQuantity_ShouldReduceStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var originalStock = product.StockQuantity;
        var reduceBy = 10;

        // Act
        product.ReduceStock(reduceBy);

        // Assert
        product.StockQuantity.Should().Be(originalStock - reduceBy);
        product.GetDomainEvents().Should().Contain(e => e is ProductStockReducedEvent);
    }

    [Fact]
    public void ReduceStock_WithInsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = CreateTestProduct();
        var reduceBy = product.StockQuantity + 1;

        // Act & Assert
        var act = () => product.ReduceStock(reduceBy);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage($"Insufficient stock. Available: {product.StockQuantity}, Requested: {reduceBy}");
    }

    [Fact]
    public void ReduceStock_WithZeroQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act & Assert
        var act = () => product.ReduceStock(0);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Quantity must be positive*");
    }

    [Fact]
    public void RestoreStock_WithValidQuantity_ShouldRestoreStock()
    {
        // Arrange
        var product = CreateTestProduct();
        var originalStock = product.StockQuantity;
        var restoreBy = 20;

        // Act
        product.RestoreStock(restoreBy);

        // Assert
        product.StockQuantity.Should().Be(originalStock + restoreBy);
        product.GetDomainEvents().Should().Contain(e => e is ProductStockRestoredEvent);
    }

    [Fact]
    public void AddImage_WithValidUrl_ShouldAddImage()
    {
        // Arrange
        var product = CreateTestProduct();
        var imageUrl = "https://example.com/image.jpg";

        // Act
        product.AddImage(imageUrl);

        // Assert
        product.ImageUrls.Should().Contain(imageUrl);
    }

    [Fact]
    public void AddImage_WithDuplicateUrl_ShouldNotAddDuplicate()
    {
        // Arrange
        var product = CreateTestProduct();
        var imageUrl = "https://example.com/image.jpg";
        product.AddImage(imageUrl);
        var initialCount = product.ImageUrls.Count;

        // Act
        product.AddImage(imageUrl);

        // Assert
        product.ImageUrls.Count.Should().Be(initialCount);
    }

    [Fact]
    public void RemoveImage_WithExistingUrl_ShouldRemoveImage()
    {
        // Arrange
        var product = CreateTestProduct();
        var imageUrl = "https://example.com/image.jpg";
        product.AddImage(imageUrl);

        // Act
        product.RemoveImage(imageUrl);

        // Assert
        product.ImageUrls.Should().NotContain(imageUrl);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateProduct()
    {
        // Arrange
        var product = CreateTestProduct();
        product.IsActive.Should().BeTrue();

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
        product.GetDomainEvents().Should().Contain(e => e is ProductDeactivatedEvent);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateProduct()
    {
        // Arrange
        var product = CreateTestProduct();
        product.Deactivate();
        product.ClearDomainEvents();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
        product.GetDomainEvents().Should().Contain(e => e is ProductActivatedEvent);
    }

    [Fact]
    public void IsInStock_WithPositiveStock_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var isInStock = product.IsInStock();

        // Assert
        isInStock.Should().BeTrue();
    }

    [Fact]
    public void IsInStock_WithZeroStock_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateTestProduct();
        product.UpdateStock(0);

        // Act
        var isInStock = product.IsInStock();

        // Assert
        isInStock.Should().BeFalse();
    }

    [Fact]
    public void HasSufficientStock_WithSufficientStock_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();
        var requestedQuantity = product.StockQuantity - 1;

        // Act
        var hasSufficientStock = product.HasSufficientStock(requestedQuantity);

        // Assert
        hasSufficientStock.Should().BeTrue();
    }

    [Fact]
    public void HasSufficientStock_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var product = CreateTestProduct();
        var requestedQuantity = product.StockQuantity + 1;

        // Act
        var hasSufficientStock = product.HasSufficientStock(requestedQuantity);

        // Assert
        hasSufficientStock.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, "منتج تجريبي")]
    [InlineData(false, "Test Product")]
    public void GetName_ShouldReturnCorrectLanguageName(bool isArabic, string expectedName)
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var name = product.GetName(isArabic);

        // Assert
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(true, "وصف تجريبي")]
    [InlineData(false, "Test Description")]
    public void GetDescription_ShouldReturnCorrectLanguageDescription(bool isArabic, string expectedDescription)
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        var description = product.GetDescription(isArabic);

        // Assert
        description.Should().Be(expectedDescription);
    }

    private static Product CreateTestProduct()
    {
        var price = Money.Create(10.50m, "BHD");
        var categoryId = Guid.NewGuid();
        return Product.Create("Test Product", "منتج تجريبي", "Test Description", "وصف تجريبي", price, 100, categoryId, "PRD-001");
    }
}