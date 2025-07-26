using AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace AutismCenter.Application.Tests.Features.Courses.Queries.GetUserEnrollments;

public class GetUserEnrollmentsHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserEnrollmentsHandler _handler;

    public GetUserEnrollmentsHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserEnrollmentsHandler(
            _enrollmentRepositoryMock.Object,
            _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ShouldReturnUserEnrollments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();

        var course1 = Course.Create("Course 1", "دورة 1", "Description 1", "وصف 1", 60, Money.Create(50, "USD"), "CRS-001");
        var course2 = Course.Create("Course 2", "دورة 2", "Description 2", "وصف 2", 90, Money.Create(75, "USD"), "CRS-002");

        var enrollments = new List<Enrollment>
        {
            Enrollment.CreateEnrollment(userId, courseId1, 30),
            Enrollment.CreateEnrollment(userId, courseId2, 30)
        };

        // Set up course navigation properties
        enrollments[0].GetType().GetProperty("Course")?.SetValue(enrollments[0], course1);
        enrollments[1].GetType().GetProperty("Course")?.SetValue(enrollments[1], course2);

        var query = new GetUserEnrollmentsQuery(userId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User enrollments retrieved successfully");
        result.Enrollments.Should().HaveCount(2);
        result.Enrollments.Should().AllSatisfy(e => e.UserId.Should().Be(userId));

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserEnrollmentsQuery(userId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found");
        result.Enrollments.Should().BeEmpty();
        result.Errors.Should().ContainKey(nameof(query.UserId));
        result.Errors![nameof(query.UserId)].Should().Contain("User with the specified ID does not exist");

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveOnlyFilter_ShouldReturnOnlyActiveEnrollments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();

        var course1 = Course.Create("Course 1", "دورة 1", "Description 1", "وصف 1", 60, Money.Create(50, "USD"), "CRS-001");
        var course2 = Course.Create("Course 2", "دورة 2", "Description 2", "وصف 2", 90, Money.Create(75, "USD"), "CRS-002");

        var enrollments = new List<Enrollment>
        {
            Enrollment.CreateEnrollment(userId, courseId1, 30),
            Enrollment.CreateEnrollment(userId, courseId2, 30)
        };

        // Deactivate one enrollment
        enrollments[1].Deactivate();

        // Set up course navigation properties
        enrollments[0].GetType().GetProperty("Course")?.SetValue(enrollments[0], course1);
        enrollments[1].GetType().GetProperty("Course")?.SetValue(enrollments[1], course2);

        var query = new GetUserEnrollmentsQuery(userId, ActiveOnly: true);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Enrollments.Should().HaveCount(1);
        result.Enrollments.Should().AllSatisfy(e => e.IsActive.Should().BeTrue());

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExcludeExpiredFilter_ShouldReturnOnlyNonExpiredEnrollments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId1 = Guid.NewGuid();
        var courseId2 = Guid.NewGuid();

        var course1 = Course.Create("Course 1", "دورة 1", "Description 1", "وصف 1", 60, Money.Create(50, "USD"), "CRS-001");
        var course2 = Course.Create("Course 2", "دورة 2", "Description 2", "وصف 2", 90, Money.Create(75, "USD"), "CRS-002");

        var enrollments = new List<Enrollment>
        {
            Enrollment.CreateEnrollment(userId, courseId1, 30), // Valid enrollment
            Enrollment.CreateEnrollment(userId, courseId2, -1)  // Expired enrollment (negative days)
        };

        // Set up course navigation properties
        enrollments[0].GetType().GetProperty("Course")?.SetValue(enrollments[0], course1);
        enrollments[1].GetType().GetProperty("Course")?.SetValue(enrollments[1], course2);

        var query = new GetUserEnrollmentsQuery(userId, IncludeExpired: false);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Enrollments.Should().HaveCount(1);
        result.Enrollments.Should().AllSatisfy(e => e.IsExpired.Should().BeFalse());

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoEnrollments_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserEnrollmentsQuery(userId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Enrollment>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("User enrollments retrieved successfully");
        result.Enrollments.Should().BeEmpty();

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserEnrollmentsQuery(userId);

        _userRepositoryMock.Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _enrollmentRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving user enrollments");
        result.Enrollments.Should().BeEmpty();
        result.Errors.Should().ContainKey("Error");
        result.Errors!["Error"].Should().Contain("Database error");

        _enrollmentRepositoryMock.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}