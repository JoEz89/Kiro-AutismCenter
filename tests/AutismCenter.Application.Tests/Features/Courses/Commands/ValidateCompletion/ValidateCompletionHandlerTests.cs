using Xunit;
using Moq;
using AutismCenter.Application.Features.Courses.Commands.ValidateCompletion;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Tests.Features.Courses.Commands.ValidateCompletion;

public class ValidateCompletionHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly ValidateCompletionHandler _handler;

    public ValidateCompletionHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _handler = new ValidateCompletionHandler(_enrollmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ReturnsFailureResponse()
    {
        // Arrange
        var command = new ValidateCompletionCommand(Guid.NewGuid());
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Enrollment not found", result.Message);
        Assert.False(result.IsCompleted);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(nameof(command.EnrollmentId), result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_EnrollmentInactive_ReturnsFailureResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.Deactivate(); // Make enrollment inactive
        
        var command = new ValidateCompletionCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot validate completion for inactive or expired enrollment", result.Message);
        Assert.False(result.IsCompleted);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Access", result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_CourseNotCompleted_ReturnsNotCompletedResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        var command = new ValidateCompletionCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Course is not yet completed", result.Message);
        Assert.False(result.IsCompleted);
        Assert.NotNull(result.Enrollment);
    }

    [Fact]
    public async Task Handle_CourseCompleted_ReturnsCompletedResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted(); // Mark as completed
        
        var command = new ValidateCompletionCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Course is completed", result.Message);
        Assert.True(result.IsCompleted);
        Assert.NotNull(result.Enrollment);
    }

    [Fact]
    public async Task Handle_CourseCompletedButNotMarked_MarksAsCompletedAndReturnsResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        
        // Create a course with one module
        var course = Course.Create(
            "Test Course",
            "دورة تجريبية",
            "Test Description",
            "وصف تجريبي",
            120,
            Money.Create(100, "USD"),
            "CRS-001"
        );
        
        var module = CourseModule.Create(
            courseId,
            "Module 1",
            "الوحدة 1",
            "video1.mp4",
            30,
            1,
            "Description 1",
            "وصف 1"
        );
        
        // Set the module ID to match what we'll use for progress
        var moduleIdProperty = typeof(CourseModule).GetProperty("Id");
        moduleIdProperty?.SetValue(module, moduleId);
        
        // Add module to course using reflection
        var modulesField = typeof(Course).GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var modulesList = (List<CourseModule>)modulesField?.GetValue(course)!;
        modulesList.Add(module);
        
        // Set course property using reflection
        var courseProperty = typeof(Enrollment).GetProperty("Course");
        courseProperty?.SetValue(enrollment, course);
        
        // Add module progress that shows completion
        enrollment.UpdateProgress(moduleId, 100);
        
        // Reset completion date to simulate a scenario where progress is 100% but not marked as completed
        var completionDateProperty = typeof(Enrollment).GetProperty("CompletionDate");
        completionDateProperty?.SetValue(enrollment, null);
        
        var command = new ValidateCompletionCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _enrollmentRepositoryMock.Setup(x => x.UpdateAsync(enrollment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Course is completed", result.Message);
        Assert.True(result.IsCompleted);
        Assert.NotNull(result.Enrollment);
        
        // Verify enrollment was updated
        _enrollmentRepositoryMock.Verify(x => x.UpdateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFailureResponse()
    {
        // Arrange
        var command = new ValidateCompletionCommand(Guid.NewGuid());
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while validating course completion", result.Message);
        Assert.False(result.IsCompleted);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Error", result.ValidationErrors.Keys);
    }
}