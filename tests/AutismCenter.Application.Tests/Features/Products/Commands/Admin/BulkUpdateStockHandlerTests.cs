using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.BulkUpdateStock;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class BulkUpdateStockHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly BulkUpdateStockHandler _handler;

    public BulkUpdateStockHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new BulkUpdateStockHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesAllProducts()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product1 = Product.Create(
            "Product 1", "منتج 1", "Description 1", "وصف 1",
            Money.Create(100m, "USD"), 10, categoryId, "PRD-001");
        var product2 = Product.Create(
            "Product 2", "منتج 2", "Description 2", "وصف 2",
            Money.Create(200m, "USD"), 20, categoryId, "PRD-002");

        var command = new BulkUpdateStockCommand(new[]
        {
            new StockUpdateItem(product1Id, 15),
            new StockUpdateItem(product2Id, 25)
        });

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalUpdated);
        Assert.Equal(0, result.TotalFailed);
        Assert.Equal(2, result.Results.Count());

        var results = result.Results.ToList();
        Assert.All(results, r => Assert.True(r.Success));
        Assert.All(results, r => Assert.Null(r.ErrorMessage));

        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new BulkUpdateStockCommand(new[]
        {
            new StockUpdateItem(productId, 15)
        });

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.TotalUpdated);
        Assert.Equal(1, result.TotalFailed);
        
        var failedResult = result.Results.First();
        Assert.False(failedResult.Success);
        Assert.Equal("Product not found", failedResult.ErrorMessage);
        Assert.Equal(productId, failedResult.ProductId);
    }

    [Fact]
    public async Task Handle_MixedResults_ReturnsCorrectCounts()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var product1 = Product.Create(
            "Product 1", "منتج 1", "Description 1", "وصف 1",
            Money.Create(100m, "USD"), 10, categoryId, "PRD-001");

        var command = new BulkUpdateStockCommand(new[]
        {
            new StockUpdateItem(product1Id, 15),
            new StockUpdateItem(product2Id, 25) // This product doesn't exist
        });

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalUpdated);
        Assert.Equal(1, result.TotalFailed);
        Assert.Equal(2, result.Results.Count());

        var successResult = result.Results.First(r => r.Success);
        Assert.Equal(product1Id, successResult.ProductId);
        Assert.Equal(10, successResult.OldQuantity);
        Assert.Equal(15, successResult.NewQuantity);

        var failedResult = result.Results.First(r => !r.Success);
        Assert.Equal(product2Id, failedResult.ProductId);
        Assert.Equal("Product not found", failedResult.ErrorMessage);
    }
}