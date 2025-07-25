using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Commands.UpdateStock;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Commands.UpdateStock;

public class UpdateStockHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateStockHandler _handler;

    public UpdateStockHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateStockHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateStockSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "USD"),
            10, // Initial stock
            categoryId,
            "PRD-001"
        );

        var command = new UpdateStockCommand(productId, 25);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(product.Id); // Use the actual product ID from the created product
        result.OldQuantity.Should().Be(10);
        result.NewQuantity.Should().Be(25);
        result.Message.Should().Be("Stock updated successfully");

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateStockCommand(productId, 25);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Product not found");
        
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ZeroStock_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "USD"),
            10,
            categoryId,
            "PRD-001"
        );

        var command = new UpdateStockCommand(productId, 0);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OldQuantity.Should().Be(10);
        result.NewQuantity.Should().Be(0);
        result.Message.Should().Be("Stock updated successfully");
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(10, 5)]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    public async Task Handle_DifferentStockValues_ShouldUpdateCorrectly(int initialStock, int newStock)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "USD"),
            initialStock,
            categoryId,
            "PRD-001"
        );

        var command = new UpdateStockCommand(productId, newStock);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OldQuantity.Should().Be(initialStock);
        result.NewQuantity.Should().Be(newStock);
    }

    [Fact]
    public async Task Handle_LargeStockIncrease_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            "Test Product",
            "منتج تجريبي",
            "Test Description",
            "وصف تجريبي",
            Money.Create(100, "USD"),
            10,
            categoryId,
            "PRD-001"
        );

        var command = new UpdateStockCommand(productId, 1000);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OldQuantity.Should().Be(10);
        result.NewQuantity.Should().Be(1000);
        result.Message.Should().Be("Stock updated successfully");
    }
}
