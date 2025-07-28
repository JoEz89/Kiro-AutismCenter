using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Queries.Admin.GetCategoriesAdmin;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Tests.Features.Products.Queries.Admin;

public class GetCategoriesAdminHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetCategoriesAdminHandler _handler;

    public GetCategoriesAdminHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetCategoriesAdminHandler(_categoryRepositoryMock.Object, _productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsGetCategoriesAdminResponse()
    {
        // Arrange
        var category1 = Category.Create("Category 1", "فئة 1", "Description 1", "وصف 1");
        var category2 = Category.Create("Category 2", "فئة 2", "Description 2", "وصف 2");
        category2.Deactivate();

        var categories = new List<Category> { category1, category2 };

        var product1 = Product.Create(
            "Product 1",
            "منتج 1",
            "Description 1",
            "وصف 1",
            Money.Create(50.00m, "USD"),
            10,
            category1.Id,
            "PRD-001");

        var product2 = Product.Create(
            "Product 2",
            "منتج 2",
            "Description 2",
            "وصف 2",
            Money.Create(75.00m, "USD"),
            5,
            category1.Id,
            "PRD-002");
        product2.Deactivate();

        var query = new GetCategoriesAdminQuery(null, true);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _productRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(category1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product1, product2 });

        _productRepositoryMock
            .Setup(x => x.GetByCategoryIdAsync(category2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Categories.Count());

        var category1Dto = result.Categories.First(c => c.Id == category1.Id);
        Assert.Equal("Category 1", category1Dto.NameEn);
        Assert.Equal("فئة 1", category1Dto.NameAr);
        Assert.True(category1Dto.IsActive);
        Assert.Equal(2, category1Dto.ProductCount);
        Assert.Equal(1, category1Dto.ActiveProductCount);

        var category2Dto = result.Categories.First(c => c.Id == category2.Id);
        Assert.Equal("Category 2", category2Dto.NameEn);
        Assert.False(category2Dto.IsActive);
        Assert.Equal(0, category2Dto.ProductCount);
        Assert.Equal(0, category2Dto.ActiveProductCount);
    }

    [Fact]
    public async Task Handle_FilterByActiveStatus_ReturnsFilteredResults()
    {
        // Arrange
        var activeCategory = Category.Create("Active Category", "فئة نشطة");
        var inactiveCategory = Category.Create("Inactive Category", "فئة غير نشطة");
        inactiveCategory.Deactivate();

        var categories = new List<Category> { activeCategory, inactiveCategory };

        var query = new GetCategoriesAdminQuery(true, false);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Categories);
        Assert.Equal(activeCategory.Id, result.Categories.First().Id);
        Assert.True(result.Categories.First().IsActive);
    }

    [Fact]
    public async Task Handle_IncludeProductCountFalse_DoesNotQueryProducts()
    {
        // Arrange
        var category = Category.Create("Test Category", "فئة اختبار");
        var categories = new List<Category> { category };

        var query = new GetCategoriesAdminQuery(null, false);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Categories);
        Assert.Equal(0, result.Categories.First().ProductCount);
        Assert.Equal(0, result.Categories.First().ActiveProductCount);

        _productRepositoryMock.Verify(x => x.GetByCategoryIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}