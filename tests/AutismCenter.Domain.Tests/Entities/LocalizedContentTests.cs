using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using Xunit;

namespace AutismCenter.Domain.Tests.Entities;

public class LocalizedContentTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLocalizedContent()
    {
        // Arrange
        var key = "welcome_message";
        var language = Language.English;
        var content = "Welcome to our website";
        var category = "ui";
        var description = "Welcome message for homepage";
        var createdBy = "admin";

        // Act
        var localizedContent = new LocalizedContent(key, language, content, category, description, createdBy);

        // Assert
        Assert.Equal(key, localizedContent.Key);
        Assert.Equal(language, localizedContent.Language);
        Assert.Equal(content, localizedContent.Content);
        Assert.Equal(category, localizedContent.Category);
        Assert.Equal(description, localizedContent.Description);
        Assert.Equal(createdBy, localizedContent.CreatedBy);
        Assert.True(localizedContent.IsActive);
        Assert.True(localizedContent.Id != Guid.Empty);
    }

    [Fact]
    public void Constructor_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        string key = null!;
        var language = Language.English;
        var content = "Welcome to our website";
        var category = "ui";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new LocalizedContent(key, language, content, category));
    }

    [Fact]
    public void Constructor_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Arrange
        var key = "welcome_message";
        var language = Language.English;
        string content = null!;
        var category = "ui";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new LocalizedContent(key, language, content, category));
    }

    [Fact]
    public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var key = "welcome_message";
        var language = Language.English;
        var content = "Welcome to our website";
        string category = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new LocalizedContent(key, language, content, category));
    }

    [Fact]
    public void UpdateContent_WithValidParameters_ShouldUpdateContentAndTimestamp()
    {
        // Arrange
        var localizedContent = new LocalizedContent("key", Language.English, "old content", "category");
        var newContent = "new content";
        var updatedBy = "admin";
        var originalUpdatedAt = localizedContent.UpdatedAt;

        // Act
        localizedContent.UpdateContent(newContent, updatedBy);

        // Assert
        Assert.Equal(newContent, localizedContent.Content);
        Assert.Equal(updatedBy, localizedContent.UpdatedBy);
        Assert.True(localizedContent.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateContent_WithNullContent_ShouldThrowArgumentNullException()
    {
        // Arrange
        var localizedContent = new LocalizedContent("key", Language.English, "content", "category");
        string newContent = null!;
        var updatedBy = "admin";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            localizedContent.UpdateContent(newContent, updatedBy));
    }

    [Fact]
    public void UpdateDescription_WithValidParameters_ShouldUpdateDescriptionAndTimestamp()
    {
        // Arrange
        var localizedContent = new LocalizedContent("key", Language.English, "content", "category");
        var newDescription = "new description";
        var updatedBy = "admin";
        var originalUpdatedAt = localizedContent.UpdatedAt;

        // Act
        localizedContent.UpdateDescription(newDescription, updatedBy);

        // Assert
        Assert.Equal(newDescription, localizedContent.Description);
        Assert.Equal(updatedBy, localizedContent.UpdatedBy);
        Assert.True(localizedContent.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrueAndUpdateTimestamp()
    {
        // Arrange
        var localizedContent = new LocalizedContent("key", Language.English, "content", "category");
        localizedContent.Deactivate("admin");
        var updatedBy = "admin";
        var originalUpdatedAt = localizedContent.UpdatedAt;

        // Act
        localizedContent.Activate(updatedBy);

        // Assert
        Assert.True(localizedContent.IsActive);
        Assert.Equal(updatedBy, localizedContent.UpdatedBy);
        Assert.True(localizedContent.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        // Arrange
        var localizedContent = new LocalizedContent("key", Language.English, "content", "category");
        var updatedBy = "admin";
        var originalUpdatedAt = localizedContent.UpdatedAt;

        // Act
        localizedContent.Deactivate(updatedBy);

        // Assert
        Assert.False(localizedContent.IsActive);
        Assert.Equal(updatedBy, localizedContent.UpdatedBy);
        Assert.True(localizedContent.UpdatedAt > originalUpdatedAt);
    }
}