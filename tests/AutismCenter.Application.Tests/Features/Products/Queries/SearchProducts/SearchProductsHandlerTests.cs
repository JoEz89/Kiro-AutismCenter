using AutismCenter.Application.Features.Products.Queries.SearchProducts;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Products.Queries.SearchProducts;

public class SearchProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly SearchProductsHandler _handler;

    public SearchProductsHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new SearchProductsHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Laptop Computer", "كمبيوتر محمول", "High-performance laptop", "لابتوب عالي الأداء", Money.Create(999.99m, "USD"), 5, categoryId, "PRD-001"),
            Product.Create("Desktop Computer", "كمبيوتر مكتبي", "Powerful desktop", "كمبيوتر مكتبي قوي", Money.Create(1299.99m, "USD"), 3, categoryId, "PRD-002")
        };

        var query = new SearchProductsQuery(
            SearchTerm: "computer",
            PageNumber: 1,
            PageSize: 10,
            CategoryId: categoryId,
            IsActive: true,
            InStockOnly: true,
            MinPrice: 500,
            MaxPrice: 2000,
            SortBy: "name",
            SortDescending: false
        );

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                query.SearchTerm,
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
        result.SearchTerm.Should().Be("computer");
        result.Products.Should().NotBeNull();
        result.Products.Items.Should().HaveCount(2);
        result.Products.TotalCount.Should().Be(2);
        result.Products.PageNumber.Should().Be(1);
        result.Products.PageSize.Should().Be(10);

        var firstProduct = result.Products.Items.First();
        firstProduct.NameEn.Should().Be("Laptop Computer");
        firstProduct.NameAr.Should().Be("كمبيوتر محمول");
        firstProduct.Price.Should().Be(999.99m);
        firstProduct.Currency.Should().Be("USD");
        firstProduct.StockQuantity.Should().Be(5);
        firstProduct.ProductSku.Should().Be("PRD-001");
    }

    [Fact]
    public async Task Handle_NoMatchingProducts_ShouldReturnEmptyResult()
    {
        // Arrange
        var query = new SearchProductsQuery(
            SearchTerm: "nonexistent",
            PageNumber: 1,
            PageSize: 10
        );

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                It.IsAny<string>(),
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
        result.SearchTerm.Should().Be("nonexistent");
        result.Products.Items.Should().BeEmpty();
        result.Products.TotalCount.Should().Be(0);
        result.Products.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ArabicSearchTerm_ShouldSearchCorrectly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Book", "كتاب", "Programming book", "كتاب برمجة", Money.Create(29.99m, "USD"), 10, categoryId, "PRD-003")
        };

        var query = new SearchProductsQuery(
            SearchTerm: "كتاب",
            PageNumber: 1,
            PageSize: 10
        );

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                "كتاب",
                1,
                10,
                null,
                null,
                null,
                null,
                null,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SearchTerm.Should().Be("كتاب");
        result.Products.Items.Should().HaveCount(1);
        result.Products.Items.First().NameAr.Should().Be("كتاب");
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllParametersToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new SearchProductsQuery(
            SearchTerm: "laptop",
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

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                It.IsAny<string>(),
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
        _productRepositoryMock.Verify(x => x.SearchPagedAsync(
            "laptop",
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
    public async Task Handle_DefaultParameters_ShouldUseCorrectDefaults()
    {
        // Arrange
        var query = new SearchProductsQuery("test");

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                It.IsAny<string>(),
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
        _productRepositoryMock.Verify(x => x.SearchPagedAsync(
            "test",
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
    public async Task Handle_MultiplePages_ShouldReturnCorrectPaginationInfo()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", "منتج 1", "Description 1", "وصف 1", Money.Create(100, "USD"), 10, categoryId, "PRD-001")
        };

        var query = new SearchProductsQuery("product", PageNumber: 3, PageSize: 2);

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                It.IsAny<string>(),
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
            .ReturnsAsync((products, 10)); // Total of 10 items

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Products.PageNumber.Should().Be(3);
        result.Products.PageSize.Should().Be(2);
        result.Products.TotalCount.Should().Be(10);
        result.Products.TotalPages.Should().Be(5);
        result.Products.HasPreviousPage.Should().BeTrue();
        result.Products.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CaseInsensitiveSearch_ShouldWork()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("LAPTOP", "لابتوب", "Description", "وصف", Money.Create(100, "USD"), 10, categoryId, "PRD-001")
        };

        var query = new SearchProductsQuery("laptop"); // lowercase search term

        _productRepositoryMock.Setup(x => x.SearchPagedAsync(
                "laptop",
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
            .ReturnsAsync((products, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Products.Items.Should().HaveCount(1);
        result.Products.Items.First().NameEn.Should().Be("LAPTOP");
    }
}
