using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.CreateProductAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class CreateProductAdminHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateProductAdminHandler _handler;

    public CreateProductAdminHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new CreateProductAdminHandler(_productRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsCreateProductAdminResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var command = new CreateProductAdminCommand(
            "Test Product",
            "منتج اختبار",
            "Test Description",
            "وصف اختبار",
            100.00m,
            "USD",
            50,
            categoryId,
            "PRD-001",
            new[] { "https://example.com/image1.jpg" },
            true);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("PRD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.NameEn);
        Assert.Equal("منتج اختبار", result.NameAr);
        Assert.Equal("PRD-001", result.ProductSku);
        Assert.Equal(100.00m, result.Price);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(50, result.StockQuantity);
        Assert.True(result.IsActive);

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductAdminCommand(
            "Test Product",
            "منتج اختبار",
            "Test Description",
            "وصف اختبار",
            100.00m,
            "USD",
            50,
            categoryId,
            "PRD-001");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsValidationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var existingProduct = Product.Create(
            "Existing Product",
            "منتج موجود",
            "Description",
            "وصف",
            Money.Create(50.00m, "USD"),
            10,
            categoryId,
            "PRD-001");

        var command = new CreateProductAdminCommand(
            "Test Product",
            "منتج اختبار",
            "Test Description",
            "وصف اختبار",
            100.00m,
            "USD",
            50,
            categoryId,
            "PRD-001");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("PRD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InactiveProduct_CreatesInactiveProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var command = new CreateProductAdminCommand(
            "Test Product",
            "منتج اختبار",
            "Test Description",
            "وصف اختبار",
            100.00m,
            "USD",
            50,
            categoryId,
            "PRD-001",
            null,
            false);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.GetBySkuAsync("PRD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsActive);
    }
}