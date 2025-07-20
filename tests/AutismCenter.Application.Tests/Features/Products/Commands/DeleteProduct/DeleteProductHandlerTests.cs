using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.Products.Commands.DeleteProduct;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Commands.DeleteProduct;

public class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteProductHandler(
            _productRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailureResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Product not found");

        _orderRepositoryMock.Verify(x => x.HasProductBeenOrderedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductHasOrders_ShouldDeactivateProduct()
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

        var command = new DeleteProductCommand(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock.Setup(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Product has been deactivated as it has existing orders");

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductHasNoOrders_ShouldDeleteProduct()
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

        var command = new DeleteProductCommand(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock.Setup(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Product deleted successfully");

        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductIsActive_ShouldCheckOrdersAndProceed()
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
        // Product is active by default

        var command = new DeleteProductCommand(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock.Setup(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _orderRepositoryMock.Verify(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductIsInactive_ShouldStillCheckOrdersAndProceed()
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
        product.Deactivate(); // Make product inactive

        var command = new DeleteProductCommand(productId);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock.Setup(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Product deleted successfully");
        _orderRepositoryMock.Verify(x => x.HasProductBeenOrderedAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }
}