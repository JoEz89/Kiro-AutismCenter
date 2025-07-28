using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.DeleteCategory;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteCategoryHandler(_categoryRepositoryMock.Object, _productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsDeleteCategoryResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var command = new DeleteCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.HasProductsInCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("Test Category", result.Message);

        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CategoryHasProducts_ThrowsValidationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var command = new DeleteCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepositoryMock
            .Setup(x => x.HasProductsInCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Cannot delete category that contains products", exception.Message);
    }
}