using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.ContentManagement.Commands;

public class CreateLocalizedContentHandlerTests
{
    private readonly Mock<ILocalizedContentRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly CreateLocalizedContentHandler _handler;

    public CreateLocalizedContentHandlerTests()
    {
        _mockRepository = new Mock<ILocalizedContentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _handler = new CreateLocalizedContentHandler(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateLocalizedContent()
    {
        // Arrange
        var command = new CreateLocalizedContentCommand(
            "test.key",
            Language.English,
            "Test content",
            "ui",
            "Test description");

        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync(command.Key, command.Language))
            .ReturnsAsync((LocalizedContent?)null);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Key, result.Key);
        Assert.Equal(command.Language, result.Language);
        Assert.Equal(command.Content, result.Content);
        Assert.Equal(command.Category, result.Category);
        Assert.Equal(command.Description, result.Description);
        Assert.True(result.IsActive);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<LocalizedContent>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingContent_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateLocalizedContentCommand(
            "existing.key",
            Language.English,
            "Test content",
            "ui");

        var existingContent = new LocalizedContent(
            command.Key,
            command.Language,
            "Existing content",
            command.Category);

        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _mockCurrentUserService.Setup(x => x.Email).Returns("admin@test.com");
        _mockRepository.Setup(x => x.GetByKeyAndLanguageAsync(command.Key, command.Language))
            .ReturnsAsync(existingContent);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => 
            _handler.Handle(command, CancellationToken.None));

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<LocalizedContent>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new CreateLocalizedContentCommand(
            "test.key",
            Language.English,
            "Test content",
            "ui");

        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}