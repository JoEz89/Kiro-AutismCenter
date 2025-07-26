using AutismCenter.Application.Features.Courses.Commands.UpdateCourse;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseHandlerTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly UpdateCourseHandler _handler;

    public UpdateCourseHandlerTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _handler = new UpdateCourseHandler(_courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateCourseSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Create(
            "Old Title",
            "عنوان قديم",
            "Old Description",
            "وصف قديم",
            60,
            Money.Create(50, "USD"),
            "CRS-001"
        );

        var command = new UpdateCourseCommand(
            courseId,
            "Updated Title",
            "عنوان محدث",
            "Updated Description",
            "وصف محدث",
            120,
            99.99m,
            "USD",
            "https://example.com/new-thumbnail.jpg"
        );

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCourse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course updated successfully");
        result.Course.Should().NotBeNull();
        result.Course!.TitleEn.Should().Be(command.TitleEn);
        result.Course.TitleAr.Should().Be(command.TitleAr);
        result.Course.DescriptionEn.Should().Be(command.DescriptionEn);
        result.Course.DescriptionAr.Should().Be(command.DescriptionAr);
        result.Course.DurationInMinutes.Should().Be(command.DurationInMinutes);
        result.Course.Price.Should().Be(command.Price);
        result.Course.Currency.Should().Be(command.Currency);
        result.Course.ThumbnailUrl.Should().Be(command.ThumbnailUrl);

        _courseRepositoryMock.Verify(x => x.UpdateAsync(existingCourse, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CourseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var command = new UpdateCourseCommand(
            courseId,
            "Updated Title",
            "عنوان محدث",
            "Updated Description",
            "وصف محدث",
            120,
            99.99m,
            "USD"
        );

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found");
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(command.Id));
        result.Errors![nameof(command.Id)].Should().Contain("Course with the specified ID does not exist");

        _courseRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutThumbnail_ShouldUpdateCourseSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Create(
            "Old Title",
            "عنوان قديم",
            "Old Description",
            "وصف قديم",
            60,
            Money.Create(50, "USD"),
            "CRS-001"
        );

        var command = new UpdateCourseCommand(
            courseId,
            "Updated Title",
            "عنوان محدث",
            "Updated Description",
            "وصف محدث",
            120,
            99.99m,
            "USD"
            // No thumbnail provided
        );

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCourse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        _courseRepositoryMock.Verify(x => x.UpdateAsync(existingCourse, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "Valid Arabic", "Valid Description", "وصف صحيح")]
    [InlineData("Valid English", "", "Valid Description", "وصف صحيح")]
    [InlineData("Valid English", "Valid Arabic", "", "وصف صحيح")]
    [InlineData("Valid English", "Valid Arabic", "Valid Description", "")]
    public async Task Handle_InvalidInput_ShouldReturnFailure(string titleEn, string titleAr, string descriptionEn, string descriptionAr)
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Create(
            "Old Title",
            "عنوان قديم",
            "Old Description",
            "وصف قديم",
            60,
            Money.Create(50, "USD"),
            "CRS-001"
        );

        var command = new UpdateCourseCommand(
            courseId,
            titleEn,
            titleAr,
            descriptionEn,
            descriptionAr,
            120,
            99.99m,
            "USD"
        );

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCourse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey("ValidationError");

        _courseRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_InvalidDuration_ShouldReturnFailure(int duration)
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = Course.Create(
            "Old Title",
            "عنوان قديم",
            "Old Description",
            "وصف قديم",
            60,
            Money.Create(50, "USD"),
            "CRS-001"
        );

        var command = new UpdateCourseCommand(
            courseId,
            "Updated Title",
            "عنوان محدث",
            "Updated Description",
            "وصف محدث",
            duration,
            99.99m,
            "USD"
        );

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCourse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey("ValidationError");

        _courseRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}