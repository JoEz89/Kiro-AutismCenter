using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.DeleteProductAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class DeleteProductAdminHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly DeleteProductAdminHandler _handler;

    public DeleteProductAdminHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new DeleteProductAdminHandler(_productRepositoryMock.Object, _orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ProductWithoutOrders_DeletesProduct()
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

        var command = new DeleteProductAdminCommand(productId);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(x => x.HasOrdersForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("successfully deleted", result.Message);

        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductWithOrders_DeactivatesProduct()
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

        var command = new DeleteProductAdminCommand(productId);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(x => x.HasOrdersForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("deactivated", result.Message);
        Assert.Contains("preserve order history", result.Message);

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductAdminCommand(productId);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}