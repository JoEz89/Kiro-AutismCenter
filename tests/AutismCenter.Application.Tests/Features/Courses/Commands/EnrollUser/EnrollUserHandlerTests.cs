using AutismCenter.Application.Features.Courses.Commands.EnrollUser;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Commands.EnrollUser;

public class EnrollUserHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly EnrollUserHandler _handler;

    public EnrollUserHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new EnrollUserHandler(
            _enrollmentRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldEnrollUserSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
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

        var command = new EnrollUserCommand(userId, courseId, 30);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User enrolled successfully");
        result.Enrollment.Should().NotBeNull();
        result.Enrollment!.UserId.Should().Be(userId);
        result.Enrollment.CourseId.Should().Be(courseId);
        result.Enrollment.IsActive.Should().BeTrue();
        result.Enrollment.ProgressPercentage.Should().Be(0);

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollUserCommand(userId, courseId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Enrollment.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(command.UserId));
        result.Errors![nameof(command.UserId)].Should().Contain("User with the specified ID does not exist");

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CourseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var command = new EnrollUserCommand(userId, courseId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course not found");
        result.Enrollment.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(command.CourseId));
        result.Errors![nameof(command.CourseId)].Should().Contain("Course with the specified ID does not exist");

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveCourse_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
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
        course.Deactivate(); // Make course inactive

        var command = new EnrollUserCommand(userId, courseId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Course is not available for enrollment");
        result.Enrollment.Should().BeNull();
        result.Errors.Should().ContainKey(nameof(command.CourseId));
        result.Errors![nameof(command.CourseId)].Should().Contain("Course is currently inactive");

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserAlreadyEnrolled_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
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

        var existingEnrollment = Enrollment.CreateEnrollment(userId, courseId, 30);

        var command = new EnrollUserCommand(userId, courseId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEnrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User is already enrolled in this course");
        result.Enrollment.Should().BeNull();
        result.Errors.Should().ContainKey("Enrollment");
        result.Errors!["Enrollment"].Should().Contain("User already has an active enrollment for this course");

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    public async Task Handle_DifferentValidityDays_ShouldEnrollUserSuccessfully(int validityDays)
    {
        // Arrange
        var userId = Guid.NewGuid();
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

        var command = new EnrollUserCommand(userId, courseId, validityDays);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserAndCourseAsync(userId, courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Enrollment!.DaysRemaining.Should().BeGreaterThan(validityDays - 2); // Allow for small time differences

        _enrollmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}