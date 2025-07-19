using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnProduct()
    {
        // Arrange
        var nameEn = "Test Product";
        var nameAr = "منتج تجريبي";
        var descriptionEn = "Test Description";
        var descriptionAr = "وصف تجريبي";
        var price = Money.Create(100, "BHD");
        var stockQuantity = 50;
        var categoryId = Guid.NewGuid();
        var productSku = "PRD-001";

        // Act
        var product = Product.Create(nameEn, nameAr, descriptionEn, descriptionAr, price, stockQuantity, categoryId, productSku);

        // Assert
        Assert.Equal(nameEn, product.NameEn);
        Assert.Equal(nameAr, product.NameAr);
        Assert.Equal(descriptionEn, product.DescriptionEn);
        Assert.Equal(descriptionAr, product.DescriptionAr);
        Assert.Equal(price, product.Price);
        Assert.Equal(stockQuantity, product.StockQuantity);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal(productSku, product.ProductSku);
        Assert.True(product.IsActive);
    }

    [Theory]
    [InlineData("", "Arabic Name", "Desc En", "Desc Ar")]
    [InlineData(" ", "Arabic Name", "Desc En", "Desc Ar")]
    [InlineData(null, "Arabic Name", "Desc En", "Desc Ar")]
    public void Create_WithEmptyEnglishName_ShouldThrowArgumentException(string nameEn, string nameAr, string descEn, string descAr)
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Product.Create(nameEn, nameAr, descEn, descAr, price, 10, categoryId, "SKU-001"));
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowArgumentException()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, -1, categoryId, "SKU-001"));
    }

    [Fact]
    public void Create_ShouldAddProductCreatedEvent()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");

        // Assert
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductCreatedEvent>(product.DomainEvents.First());
    }

    [Fact]
    public void ReduceStock_WithValidQuantity_ShouldReduceStockAndAddEvent()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");
        product.ClearDomainEvents();

        // Act
        product.ReduceStock(3);

        // Assert
        Assert.Equal(7, product.StockQuantity);
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductStockReducedEvent>(product.DomainEvents.First());
    }

    [Fact]
    public void ReduceStock_WithInsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 5, categoryId, "SKU-001");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => product.ReduceStock(10));
    }

    [Fact]
    public void RestoreStock_WithValidQuantity_ShouldIncreaseStockAndAddEvent()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 5, categoryId, "SKU-001");
        product.ClearDomainEvents();

        // Act
        product.RestoreStock(3);

        // Assert
        Assert.Equal(8, product.StockQuantity);
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductStockRestoredEvent>(product.DomainEvents.First());
    }

    [Fact]
    public void IsInStock_WithPositiveStock_ShouldReturnTrue()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 5, categoryId, "SKU-001");

        // Act
        var isInStock = product.IsInStock();

        // Assert
        Assert.True(isInStock);
    }

    [Fact]
    public void IsInStock_WithZeroStock_ShouldReturnFalse()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 0, categoryId, "SKU-001");

        // Act
        var isInStock = product.IsInStock();

        // Assert
        Assert.False(isInStock);
    }

    [Fact]
    public void HasSufficientStock_WithEnoughStock_ShouldReturnTrue()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");

        // Act
        var hasSufficientStock = product.HasSufficientStock(5);

        // Assert
        Assert.True(hasSufficientStock);
    }

    [Fact]
    public void HasSufficientStock_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 3, categoryId, "SKU-001");

        // Act
        var hasSufficientStock = product.HasSufficientStock(5);

        // Assert
        Assert.False(hasSufficientStock);
    }

    [Fact]
    public void GetName_WithEnglish_ShouldReturnEnglishName()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("English Name", "Arabic Name", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");

        // Act
        var name = product.GetName(false);

        // Assert
        Assert.Equal("English Name", name);
    }

    [Fact]
    public void GetName_WithArabic_ShouldReturnArabicName()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("English Name", "Arabic Name", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");

        // Act
        var name = product.GetName(true);

        // Assert
        Assert.Equal("Arabic Name", name);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateAndAddEvent()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");
        product.ClearDomainEvents();

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.IsActive);
        Assert.Single(product.DomainEvents);
        Assert.IsType<ProductDeactivatedEvent>(product.DomainEvents.First());
    }

    [Fact]
    public void AddImage_WithValidUrl_ShouldAddImage()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");
        var imageUrl = "https://example.com/image.jpg";

        // Act
        product.AddImage(imageUrl);

        // Assert
        Assert.Contains(imageUrl, product.ImageUrls);
    }

    [Fact]
    public void AddImage_WithDuplicateUrl_ShouldNotAddDuplicate()
    {
        // Arrange
        var price = Money.Create(100, "BHD");
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Name En", "Name Ar", "Desc En", "Desc Ar", price, 10, categoryId, "SKU-001");
        var imageUrl = "https://example.com/image.jpg";

        // Act
        product.AddImage(imageUrl);
        product.AddImage(imageUrl);

        // Assert
        Assert.Single(product.ImageUrls);
    }
}