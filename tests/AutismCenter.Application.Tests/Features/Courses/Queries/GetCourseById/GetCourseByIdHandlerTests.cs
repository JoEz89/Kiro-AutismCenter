using AutismCenter.Application.Features.Courses.Queries.GetCourseById;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Queries.GetCourseById;

public class GetCourseByIdHandlerTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly GetCourseByIdHandler _handler;

    public GetCourseByIdHandlerTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _handler = new GetCourseByIdHandler(_courseRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCourseId_ShouldReturnCourse()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Create(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction to autism spectrum disorders",
            "مقدمة شاملة لاضطرابات طيف التوحد",
            120,
            Money.Create(99.99m, "USD"),
            "CRS-001"
        );

        var query = new GetCourseByIdQuery(courseId);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Course retrieved successfully");
        result.Course.Should().NotBeNull();
        result.Course!.Id.Should().Be(course.Id);
        result.Course.TitleEn.Should().Be(course.TitleEn);
        result.Course.TitleAr.Should().Be(course.TitleAr);
        result.Course.DescriptionEn.Should().Be(course.DescriptionEn);
        result.Course.DescriptionAr.Should().Be(course.DescriptionAr);
        result.Course.DurationInMinutes.Should().Be(course.DurationInMinutes);
        result.Course.Price.Should().Be(course.Price?.Amount ?? 0);
        result.Course.Currency.Should().Be(course.Price?.Currency ?? "USD");
        result.Course.CourseCode.Should().Be(course.CourseCode);
        result.Course.IsActive.Should().Be(course.IsActive);

        _courseRepositoryMock.Verify(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CourseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetCourseByIdQuery(courseId);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found");
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(query.Id));
        result.Errors![nameof(query.Id)].Should().Contain("Course with the specified ID does not exist");

        _courseRepositoryMock.Verify(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var query = new GetCourseByIdQuery(courseId);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving the course");
        result.Course.Should().BeNull();
        result.Errors.Should().ContainKey("Error");
        result.Errors!["Error"].Should().Contain("Database error");

        _courseRepositoryMock.Verify(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CourseWithThumbnail_ShouldReturnCourseWithThumbnail()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Create(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction",
            "مقدمة شاملة",
            120,
            Money.Create(99.99m, "USD"),
            "CRS-001"
        );
        course.SetThumbnail("https://example.com/thumbnail.jpg");

        var query = new GetCourseByIdQuery(courseId);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Course!.ThumbnailUrl.Should().Be("https://example.com/thumbnail.jpg");

        _courseRepositoryMock.Verify(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveCourse_ShouldReturnCourse()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = Course.Create(
            "Introduction to Autism",
            "مقدمة في التوحد",
            "A comprehensive introduction",
            "مقدمة شاملة",
            120,
            Money.Create(99.99m, "USD"),
            "CRS-001"
        );
        course.Deactivate();

        var query = new GetCourseByIdQuery(courseId);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Course!.IsActive.Should().BeFalse();

        _courseRepositoryMock.Verify(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()), Times.Once);
    }
}