using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.ContentManagement.Commands;

public class UpdateLocalizedContentHandlerTests
{
    private readonly Mock<ILocalizedContentRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UpdateLocalizedContentHandler _handler;

    public UpdateLocalizedContentHandlerTests()
    {
        _mockRepository = new Mock<ILocalizedContentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new UpdateLocalizedContentHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateLocalizedContent()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var existingContent = new LocalizedContent(
            "test.key",
            Language.English,
            "Original content",
            "ui",
            "Original description");

        var command = new UpdateLocalizedContentCommand(
            contentId,
            "Updated content",
            "Updated description");

        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        _mockRepository.Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(existingContent);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingContent.Id, result.Id);
        Assert.Equal(command.Content, result.Content);
        Assert.Equal(command.Description, result.Description);

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LocalizedContent>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentContent_ShouldThrowNotFoundException()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var command = new UpdateLocalizedContentCommand(
            contentId,
            "Updated content");

        _mockRepository.Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync((LocalizedContent?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LocalizedContent>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldNotUpdateDescription()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var existingContent = new LocalizedContent(
            "test.key",
            Language.English,
            "Original content",
            "ui",
            "Original description");

        var command = new UpdateLocalizedContentCommand(
            contentId,
            "Updated content",
            null);

        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        _mockRepository.Setup(x => x.GetByIdAsync(contentId))
            .ReturnsAsync(existingContent);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original description", result.Description);
    }
}