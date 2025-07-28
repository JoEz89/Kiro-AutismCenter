using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.ContentManagement.Commands;

public class BulkUpdateLocalizedContentHandlerTests
{
    private readonly Mock<ILocalizedContentRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly BulkUpdateLocalizedContentHandler _handler;

    public BulkUpdateLocalizedContentHandlerTests()
    {
        _mockRepository = new Mock<ILocalizedContentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new BulkUpdateLocalizedContentHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithMixedExistingAndNewContent_ShouldUpdateAndCreateCorrectly()
    {
        // Arrange
        var existingContent = new LocalizedContent(
            "existing.key",
            Language.English,
            "Original content",
            "ui");

        var items = new List<BulkUpdateLocalizedContentItem>
        {
            new("existing.key", Language.English, "Updated content", "Updated description"),
            new("new.key", Language.English, "New content", "New description"),
            new("another.key", Language.Arabic, "محتوى جديد", "وصف جديد")
        };

        var command = new BulkUpdateLocalizedContentCommand("ui", items);

        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("existing.key", Language.English))
            .ReturnsAsync(existingContent);
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("new.key", Language.English))
            .ReturnsAsync((LocalizedContent?)null);
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("another.key", Language.Arabic))
            .ReturnsAsync((LocalizedContent?)null);
        
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.SuccessfulUpdates);
        Assert.Equal(0, result.FailedUpdates);
        Assert.All(result.Results, r => Assert.True(r.Success));

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LocalizedContent>()), Times.Once);
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<LocalizedContent>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldHandleFailuresGracefully()
    {
        // Arrange
        var items = new List<BulkUpdateLocalizedContentItem>
        {
            new("valid.key", Language.English, "Valid content"),
            new("error.key", Language.English, "Error content")
        };

        var command = new BulkUpdateLocalizedContentCommand("ui", items);

        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("valid.key", Language.English))
            .ReturnsAsync((LocalizedContent?)null);
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("error.key", Language.English))
            .ThrowsAsync(new Exception("Database error"));

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.SuccessfulUpdates);
        Assert.Equal(1, result.FailedUpdates);
        
        var successResult = result.Results.First(r => r.Key == "valid.key");
        var failureResult = result.Results.First(r => r.Key == "error.key");
        
        Assert.True(successResult.Success);
        Assert.False(failureResult.Success);
        Assert.Equal("Database error", failureResult.ErrorMessage);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<LocalizedContent>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoSuccessfulUpdates_ShouldNotSaveChanges()
    {
        // Arrange
        var items = new List<BulkUpdateLocalizedContentItem>
        {
            new("error.key", Language.English, "Error content")
        };

        var command = new BulkUpdateLocalizedContentCommand("ui", items);

        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync("error.key", Language.English))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(0, result.SuccessfulUpdates);
        Assert.Equal(1, result.FailedUpdates);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}