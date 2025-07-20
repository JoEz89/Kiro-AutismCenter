using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Commands.CreateProduct;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Commands.CreateProduct;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var command = new CreateProductCommand(
            "Laptop",
            "لابتوب",
            "High-performance laptop",
            "لابتوب عالي الأداء",
            999.99m,
            "USD",
            10,
            categoryId,
            "PRD-001",
            new[] { "https://example.com/image1.jpg", "https://example.com/image2.jpg" }
        );

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.GetBySkuAsync(command.ProductSku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var createdProduct = Product.Create(
            command.NameEn,
            command.NameAr,
            command.DescriptionEn,
            command.DescriptionAr,
            Money.Create(command.Price, command.Currency),
            command.StockQuantity,
            categoryId,
            command.ProductSku
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.NameEn.Should().Be(command.NameEn);
        result.Product.NameAr.Should().Be(command.NameAr);
        result.Product.DescriptionEn.Should().Be(command.DescriptionEn);
        result.Product.DescriptionAr.Should().Be(command.DescriptionAr);
        result.Product.Price.Should().Be(command.Price);
        result.Product.Currency.Should().Be(command.Currency);
        result.Product.StockQuantity.Should().Be(command.StockQuantity);
        result.Product.ProductSku.Should().Be(command.ProductSku);
        result.Message.Should().Be("Product created successfully");

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "Laptop",
            "لابتوب",
            "High-performance laptop",
            "لابتوب عالي الأداء",
            999.99m,
            "USD",
            10,
            categoryId,
            "PRD-001"
        );

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Category not found");
        
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingSku_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var existingProduct = Product.Create(
            "Existing Product",
            "منتج موجود",
            "Description",
            "وصف",
            Money.Create(100, "USD"),
            5,
            categoryId,
            "PRD-001"
        );

        var command = new CreateProductCommand(
            "Laptop",
            "لابتوب",
            "High-performance laptop",
            "لابتوب عالي الأداء",
            999.99m,
            "USD",
            10,
            categoryId,
            "PRD-001" // Same SKU as existing product
        );

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.GetBySkuAsync(command.ProductSku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("A product with this SKU already exists");
        
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutImages_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var command = new CreateProductCommand(
            "Laptop",
            "لابتوب",
            "High-performance laptop",
            "لابتوب عالي الأداء",
            999.99m,
            "USD",
            10,
            categoryId,
            "PRD-001"
            // No images provided
        );

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.GetBySkuAsync(command.ProductSku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var createdProduct = Product.Create(
            command.NameEn,
            command.NameAr,
            command.DescriptionEn,
            command.DescriptionAr,
            Money.Create(command.Price, command.Currency),
            command.StockQuantity,
            categoryId,
            command.ProductSku
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Product.ImageUrls.Should().BeEmpty();
        result.Message.Should().Be("Product created successfully");

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("BHD")]
    public async Task Handle_DifferentCurrencies_ShouldCreateProductSuccessfully(string currency)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var command = new CreateProductCommand(
            "Laptop",
            "لابتوب",
            "High-performance laptop",
            "لابتوب عالي الأداء",
            999.99m,
            currency,
            10,
            categoryId,
            "PRD-001"
        );

        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.GetBySkuAsync(command.ProductSku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var createdProduct = Product.Create(
            command.NameEn,
            command.NameAr,
            command.DescriptionEn,
            command.DescriptionAr,
            Money.Create(command.Price, command.Currency),
            command.StockQuantity,
            categoryId,
            command.ProductSku
        );

        _productRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Product.Currency.Should().Be(currency);
    }
}