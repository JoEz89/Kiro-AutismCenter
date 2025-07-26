using Xunit;
using Moq;
using AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Tests.Features.Courses.Queries.GetCourseCompletionStatus;

public class GetCourseCompletionStatusHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly GetCourseCompletionStatusHandler _handler;

    public GetCourseCompletionStatusHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _handler = new GetCourseCompletionStatusHandler(_enrollmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ReturnsFailureResponse()
    {
        // Arrange
        var query = new GetCourseCompletionStatusQuery(Guid.NewGuid());
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(query.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Enrollment not found", result.Message);
        Assert.Null(result.CompletionStatus);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(nameof(query.EnrollmentId), result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_ValidEnrollmentWithoutModules_ReturnsCompletionStatus()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var course = Course.Create(
            "Test Course",
            "دورة تجريبية",
            "Test Description",
            "وصف تجريبي",
            120,
            Money.Create(100, "USD"),
            "CRS-001"
        );
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        
        // Use reflection to set the Course property since it's private set
        var courseProperty = typeof(Enrollment).GetProperty("Course");
        courseProperty?.SetValue(enrollment, course);
        
        var query = new GetCourseCompletionStatusQuery(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(query.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Course completion status retrieved successfully", result.Message);
        Assert.NotNull(result.CompletionStatus);
        Assert.Equal(enrollment.Id, result.CompletionStatus.EnrollmentId);
        Assert.Equal(userId, result.CompletionStatus.UserId);
        Assert.Equal(courseId, result.CompletionStatus.CourseId);
        Assert.Equal("Test Course", result.CompletionStatus.CourseTitle);
        Assert.Equal("دورة تجريبية", result.CompletionStatus.CourseTitleAr);
        Assert.Equal(0, result.CompletionStatus.TotalModules);
        Assert.Equal(0, result.CompletionStatus.CompletedModules);
        Assert.Empty(result.CompletionStatus.ModuleCompletions);
    }

    [Fact]
    public async Task Handle_ValidEnrollmentWithModules_ReturnsDetailedCompletionStatus()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var moduleId1 = Guid.NewGuid();
        var moduleId2 = Guid.NewGuid();
        
        var course = Course.Create(
            "Test Course",
            "دورة تجريبية",
            "Test Description",
            "وصف تجريبي",
            120,
            Money.Create(100, "USD"),
            "CRS-001"
        );
        
        var module1 = CourseModule.Create(
            courseId,
            "Module 1",
            "الوحدة 1",
            "video1.mp4",
            30,
            1,
            "Description 1",
            "وصف 1"
        );
        
        var module2 = CourseModule.Create(
            courseId,
            "Module 2",
            "الوحدة 2",
            "video2.mp4",
            30,
            2,
            "Description 2",
            "وصف 2"
        );
        
        // Add modules to course using reflection
        var modulesField = typeof(Course).GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var modulesList = (List<CourseModule>)modulesField?.GetValue(course)!;
        modulesList.Add(module1);
        modulesList.Add(module2);
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        
        // Set course property
        var courseProperty = typeof(Enrollment).GetProperty("Course");
        courseProperty?.SetValue(enrollment, course);
        
        // Set the module IDs to match what we'll use for progress
        var moduleIdProperty = typeof(CourseModule).GetProperty("Id");
        moduleIdProperty?.SetValue(module1, moduleId1);
        moduleIdProperty?.SetValue(module2, moduleId2);
        
        // Add module progress
        enrollment.UpdateProgress(moduleId1, 100); // Module 1 completed
        enrollment.UpdateProgress(moduleId2, 50);  // Module 2 half completed
        
        var query = new GetCourseCompletionStatusQuery(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(query.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Course completion status retrieved successfully", result.Message);
        Assert.NotNull(result.CompletionStatus);
        Assert.Equal(2, result.CompletionStatus.TotalModules);
        Assert.Equal(1, result.CompletionStatus.CompletedModules); // Only module 1 is completed
        Assert.Equal(2, result.CompletionStatus.ModuleCompletions.Count);
        
        var module1Completion = result.CompletionStatus.ModuleCompletions.First(m => m.Order == 1);
        Assert.Equal(100, module1Completion.ProgressPercentage);
        Assert.True(module1Completion.IsCompleted);
        
        var module2Completion = result.CompletionStatus.ModuleCompletions.First(m => m.Order == 2);
        Assert.Equal(50, module2Completion.ProgressPercentage);
        Assert.False(module2Completion.IsCompleted);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFailureResponse()
    {
        // Arrange
        var query = new GetCourseCompletionStatusQuery(Guid.NewGuid());
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(query.EnrollmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while retrieving course completion status", result.Message);
        Assert.Null(result.CompletionStatus);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Error", result.ValidationErrors.Keys);
    }
}