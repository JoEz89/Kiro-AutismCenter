using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.UpdateProductAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class UpdateProductAdminHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly UpdateProductAdminHandler _handler;

    public UpdateProductAdminHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new UpdateProductAdminHandler(_productRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUpdateProductAdminResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var product = Product.Create(
            "Old Product",
            "منتج قديم",
            "Old Description",
            "وصف قديم",
            Money.Create(50.00m, "USD"),
            10,
            categoryId,
            "PRD-001");

        var command = new UpdateProductAdminCommand(
            productId,
            "Updated Product",
            "منتج محدث",
            "Updated Description",
            "وصف محدث",
            100.00m,
            "USD",
            25,
            categoryId,
            new[] { "https://example.com/image1.jpg" },
            true);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.NameEn);
        Assert.Equal("منتج محدث", result.NameAr);
        Assert.Equal("PRD-001", result.ProductSku);
        Assert.Equal(100.00m, result.Price);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(25, result.StockQuantity);
        Assert.True(result.IsActive);

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new UpdateProductAdminCommand(
            productId,
            "Updated Product",
            "منتج محدث",
            "Updated Description",
            "وصف محدث",
            100.00m,
            "USD",
            25,
            categoryId);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CategoryChanged_ValidatesNewCategory()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var newCategory = Category.Create("New Category", "فئة جديدة");
        
        var product = Product.Create(
            "Test Product",
            "منتج اختبار",
            "Description",
            "وصف",
            Money.Create(50.00m, "USD"),
            10,
            oldCategoryId,
            "PRD-001");

        var command = new UpdateProductAdminCommand(
            productId,
            "Updated Product",
            "منتج محدث",
            "Updated Description",
            "وصف محدث",
            100.00m,
            "USD",
            25,
            newCategoryId);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _categoryRepositoryMock.Verify(x => x.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeactivateProduct_UpdatesStatusCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج اختبار",
            "Description",
            "وصف",
            Money.Create(50.00m, "USD"),
            10,
            categoryId,
            "PRD-001");

        var command = new UpdateProductAdminCommand(
            productId,
            "Test Product",
            "منتج اختبار",
            "Description",
            "وصف",
            50.00m,
            "USD",
            10,
            categoryId,
            null,
            false);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsActive);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}