using AutismCenter.Application.Features.Products.Queries.GetProducts;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Queries.GetProducts;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductsHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnPagedProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "إلكترونيات");
        var products = new List<Product>
        {
            Product.Create("Product 1", "منتج 1", "Description 1", "وصف 1", Money.Create(100, "USD"), 10, categoryId, "PRD-001"),
            Product.Create("Product 2", "منتج 2", "Description 2", "وصف 2", Money.Create(200, "USD"), 5, categoryId, "PRD-002")
        };

        var query = new GetProductsQuery(
            PageNumber: 1,
            PageSize: 10,
            CategoryId: categoryId,
            IsActive: true,
            InStockOnly: true,
            MinPrice: 50,
            MaxPrice: 300,
            SortBy: "name",
            SortDescending: false
        );

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.CategoryId,
                query.IsActive,
                query.InStockOnly,
                query.MinPrice,
                query.MaxPrice,
                query.SortBy,
                query.SortDescending,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().NotBeNull();
        result.Products.Items.Should().HaveCount(2);
        result.Products.TotalCount.Should().Be(2);
        result.Products.PageNumber.Should().Be(1);
        result.Products.PageSize.Should().Be(10);
        result.Products.TotalPages.Should().Be(1);
        result.Products.HasPreviousPage.Should().BeFalse();
        result.Products.HasNextPage.Should().BeFalse();

        var firstProduct = result.Products.Items.First();
        firstProduct.NameEn.Should().Be("Product 1");
        firstProduct.NameAr.Should().Be("منتج 1");
        firstProduct.Price.Should().Be(100);
        firstProduct.Currency.Should().Be("USD");
        firstProduct.StockQuantity.Should().Be(10);
        firstProduct.ProductSku.Should().Be("PRD-001");
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new GetProductsQuery(PageNumber: 1, PageSize: 10);

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Items.Should().BeEmpty();
        result.Products.TotalCount.Should().Be(0);
        result.Products.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DefaultParameters_ShouldCallRepositoryWithCorrectDefaults()
    {
        // Arrange
        var query = new GetProductsQuery(); // Using default values

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productRepositoryMock.Verify(x => x.GetPagedAsync(
            1, // Default PageNumber
            10, // Default PageSize
            null, // Default CategoryId
            null, // Default IsActive
            null, // Default InStockOnly
            null, // Default MinPrice
            null, // Default MaxPrice
            null, // Default SortBy
            false, // Default SortDescending
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsQuery(
            PageNumber: 2,
            PageSize: 20,
            CategoryId: categoryId,
            IsActive: true,
            InStockOnly: true,
            MinPrice: 100,
            MaxPrice: 500,
            SortBy: "price",
            SortDescending: true
        );

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _productRepositoryMock.Verify(x => x.GetPagedAsync(
            2,
            20,
            categoryId,
            true,
            true,
            100,
            500,
            "price",
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultiplePages_ShouldReturnCorrectPaginationInfo()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", "منتج 1", "Description 1", "وصف 1", Money.Create(100, "USD"), 10, categoryId, "PRD-001")
        };

        var query = new GetProductsQuery(PageNumber: 2, PageSize: 1);

        _productRepositoryMock.Setup(x => x.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 5)); // Total of 5 items

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Products.PageNumber.Should().Be(2);
        result.Products.PageSize.Should().Be(1);
        result.Products.TotalCount.Should().Be(5);
        result.Products.TotalPages.Should().Be(5);
        result.Products.HasPreviousPage.Should().BeTrue();
        result.Products.HasNextPage.Should().BeTrue();
    }
}
