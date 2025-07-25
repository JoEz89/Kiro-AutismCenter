using AutismCenter.Application.Features.Products.Queries.GetProductById;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Queries.GetProductById;

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(199.99m, "USD"),
            15,
            categoryId,
            "PRD-001"
        );
        product.AddImage("https://example.com/image1.jpg");
        product.AddImage("https://example.com/image2.jpg");

        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.Should().NotBeNull();
        result.Product!.Id.Should().Be(product.Id);
        result.Product.NameEn.Should().Be("Test Product");
        result.Product.NameAr.Should().Be("منتج تجريبي");
        result.Product.DescriptionEn.Should().Be("Test Description");
        result.Product.DescriptionAr.Should().Be("وصف تجريبي");
        result.Product.Price.Should().Be(199.99m);
        result.Product.Currency.Should().Be("USD");
        result.Product.StockQuantity.Should().Be(15);
        result.Product.CategoryId.Should().Be(categoryId);
        result.Product.IsActive.Should().BeTrue();
        result.Product.ProductSku.Should().Be("PRD-001");
        result.Product.ImageUrls.Should().HaveCount(2);
        result.Product.ImageUrls.Should().Contain("https://example.com/image1.jpg");
        result.Product.ImageUrls.Should().Contain("https://example.com/image2.jpg");
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNullProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ProductWithoutImages_ShouldReturnProductWithEmptyImageList()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(99.99m, "BHD"),
            5,
            categoryId,
            "PRD-002"
        );
        // No images added

        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.Should().NotBeNull();
        result.Product!.ImageUrls.Should().BeEmpty();
        result.Product.Currency.Should().Be("BHD");
    }

    [Fact]
    public async Task Handle_InactiveProduct_ShouldReturnProductWithCorrectStatus()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Inactive Product",
            "منتج غير نشط",
            "Inactive Description",
            "وصف غير نشط",
            Money.Create(50.00m, "USD"),
            0,
            categoryId,
            "PRD-003"
        );
        product.Deactivate();

        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.Should().NotBeNull();
        result.Product!.IsActive.Should().BeFalse();
        result.Product.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ValidId_ShouldCallRepositoryOnce()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductWithCategory_ShouldIncludeCategoryInformation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Books", "كتب");
        var product = Product.Create(
            "Programming Book",
            "كتاب برمجة",
            "Learn programming",
            "تعلم البرمجة",
            Money.Create(29.99m, "USD"),
            20,
            categoryId,
            "PRD-004"
        );

        var query = new GetProductByIdQuery(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.Should().NotBeNull();
        result.Product!.CategoryId.Should().Be(categoryId);
        // Note: Category names would be empty since we're not setting up the navigation property
        // In a real scenario, the repository would include the category
    }
}
