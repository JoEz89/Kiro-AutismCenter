using Xunit;
using Moq;
using AutismCenter.Application.Features.Products.Commands.Admin.UpdateCategory;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Application.Common.Exceptions;

namespace AutismCenter.Application.Tests.Features.Products.Commands.Admin;

public class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new UpdateCategoryHandler(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUpdateCategoryResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Old Category", "فئة قديمة", "Old Description", "وصف قديم");
        var command = new UpdateCategoryCommand(
            categoryId,
            "Updated Category",
            "فئة محدثة",
            "Updated Description",
            "وصف محدث",
            true);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Category", result.NameEn);
        Assert.Equal("فئة محدثة", result.NameAr);
        Assert.Equal("Updated Description", result.DescriptionEn);
        Assert.Equal("وصف محدث", result.DescriptionAr);
        Assert.True(result.IsActive);

        _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand(
            categoryId,
            "Updated Category",
            "فئة محدثة");

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeactivateCategory_UpdatesStatusCorrectly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Test Category", "فئة اختبار");
        var command = new UpdateCategoryCommand(
            categoryId,
            "Test Category",
            "فئة اختبار",
            null,
            null,
            false);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsActive);
        _categoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}