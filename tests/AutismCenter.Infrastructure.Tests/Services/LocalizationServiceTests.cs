using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class LocalizationServiceTests
{
    private readonly Mock<ILocalizedContentRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<LocalizationService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly LocalizationService _service;

    public LocalizationServiceTests()
    {
        _mockRepository = new Mock<ILocalizedContentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<LocalizationService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _service = new LocalizationService(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetLocalizedContentAsync_WithExistingContent_ShouldReturnContent()
    {
        // Arrange
        var key = "welcome_message";
        var language = Language.English;
        var expectedContent = "Welcome to our website";
        var localizedContent = new LocalizedContent(key, language, expectedContent, "ui");

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync(localizedContent);

        // Act
        var result = await _service.GetLocalizedContentAsync(key, language);

        // Assert
        Assert.Equal(expectedContent, result);
        _mockRepository.Verify(r => r.GetByKeyAndLanguageAsync(key, language), Times.Once);
    }

    [Fact]
    public async Task GetLocalizedContentAsync_WithNonExistingContent_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "non_existing_key";
        var language = Language.English;
        var defaultValue = "Default message";

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync((LocalizedContent?)null);

        // Act
        var result = await _service.GetLocalizedContentAsync(key, language, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
        _mockRepository.Verify(r => r.GetByKeyAndLanguageAsync(key, language), Times.Once);
    }

    [Fact]
    public async Task GetLocalizedContentAsync_WithNonExistingContentAndNoDefault_ShouldReturnKey()
    {
        // Arrange
        var key = "non_existing_key";
        var language = Language.English;

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync((LocalizedContent?)null);

        // Act
        var result = await _service.GetLocalizedContentAsync(key, language);

        // Assert
        Assert.Equal(key, result);
        _mockRepository.Verify(r => r.GetByKeyAndLanguageAsync(key, language), Times.Once);
    }

    [Fact]
    public async Task GetLocalizedContentByCategoryAsync_WithExistingCategory_ShouldReturnDictionary()
    {
        // Arrange
        var category = "ui";
        var language = Language.English;
        var contents = new List<LocalizedContent>
        {
            new LocalizedContent("key1", language, "Content 1", category),
            new LocalizedContent("key2", language, "Content 2", category)
        };

        _mockRepository.Setup(r => r.GetByCategoryAsync(category, language))
            .ReturnsAsync(contents);

        // Act
        var result = await _service.GetLocalizedContentByCategoryAsync(category, language);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Content 1", result["key1"]);
        Assert.Equal("Content 2", result["key2"]);
        _mockRepository.Verify(r => r.GetByCategoryAsync(category, language), Times.Once);
    }

    [Fact]
    public async Task SetLocalizedContentAsync_WithNewContent_ShouldAddContent()
    {
        // Arrange
        var key = "new_key";
        var language = Language.English;
        var content = "New content";
        var category = "ui";
        var updatedBy = "admin";

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync((LocalizedContent?)null);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SetLocalizedContentAsync(key, language, content, category, updatedBy);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<LocalizedContent>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetLocalizedContentAsync_WithExistingContent_ShouldUpdateContent()
    {
        // Arrange
        var key = "existing_key";
        var language = Language.English;
        var newContent = "Updated content";
        var category = "ui";
        var updatedBy = "admin";
        var existingContent = new LocalizedContent(key, language, "Old content", category);

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync(existingContent);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SetLocalizedContentAsync(key, language, newContent, category, updatedBy);

        // Assert
        Assert.True(result);
        Assert.Equal(newContent, existingContent.Content);
        _mockRepository.Verify(r => r.UpdateAsync(existingContent), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLocalizedContentAsync_WithExistingContent_ShouldUpdateContent()
    {
        // Arrange
        var key = "existing_key";
        var language = Language.English;
        var newContent = "Updated content";
        var updatedBy = "admin";
        var existingContent = new LocalizedContent(key, language, "Old content", "ui");

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync(existingContent);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateLocalizedContentAsync(key, language, newContent, updatedBy);

        // Assert
        Assert.True(result);
        Assert.Equal(newContent, existingContent.Content);
        _mockRepository.Verify(r => r.UpdateAsync(existingContent), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLocalizedContentAsync_WithNonExistingContent_ShouldReturnFalse()
    {
        // Arrange
        var key = "non_existing_key";
        var language = Language.English;
        var newContent = "Updated content";
        var updatedBy = "admin";

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync((LocalizedContent?)null);

        // Act
        var result = await _service.UpdateLocalizedContentAsync(key, language, newContent, updatedBy);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<LocalizedContent>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteLocalizedContentAsync_WithExistingContent_ShouldDeleteContent()
    {
        // Arrange
        var key = "existing_key";
        var language = Language.English;
        var existingContent = new LocalizedContent(key, language, "Content", "ui");

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync(existingContent);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteLocalizedContentAsync(key, language);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(existingContent.Id), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteLocalizedContentAsync_WithNonExistingContent_ShouldReturnFalse()
    {
        // Arrange
        var key = "non_existing_key";
        var language = Language.English;

        _mockRepository.Setup(r => r.GetByKeyAndLanguageAsync(key, language))
            .ReturnsAsync((LocalizedContent?)null);

        // Act
        var result = await _service.DeleteLocalizedContentAsync(key, language);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ContentExistsAsync_WithExistingContent_ShouldReturnTrue()
    {
        // Arrange
        var key = "existing_key";
        var language = Language.English;

        _mockRepository.Setup(r => r.ExistsAsync(key, language))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ContentExistsAsync(key, language);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(key, language), Times.Once);
    }

    [Fact]
    public async Task ContentExistsAsync_WithNonExistingContent_ShouldReturnFalse()
    {
        // Arrange
        var key = "non_existing_key";
        var language = Language.English;

        _mockRepository.Setup(r => r.ExistsAsync(key, language))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ContentExistsAsync(key, language);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.ExistsAsync(key, language), Times.Once);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var expectedCategories = new List<string> { "ui", "email", "error" };

        _mockRepository.Setup(r => r.GetCategoriesAsync())
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _service.GetCategoriesAsync();

        // Assert
        Assert.Equal(expectedCategories, result);
        _mockRepository.Verify(r => r.GetCategoriesAsync(), Times.Once);
    }
}