using AutismCenter.Application.Features.Courses.Queries.GetCourses;
using AutismCenter.Application.Features.Courses.Queries.GetCourseById;
using AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;
using AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;
using AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;
using AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;
using AutismCenter.Application.Features.Courses.Commands.EnrollUser;
using AutismCenter.Application.Features.Courses.Commands.UpdateProgress;
using AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;
using AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;
using AutismCenter.Application.Features.Courses.Common;
using AutismCenter.Application.Common.Models;
using AutismCenter.WebApi.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Controllers;

public class CoursesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CoursesController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public CoursesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CoursesController();

        // Setup controller context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Create a mock HttpContext with the mediator service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ISender))).Returns(_mediatorMock.Object);
        _controller.ControllerContext.HttpContext.RequestServices = serviceProvider.Object;
    }

    [Fact]
    public async Task GetCourses_ValidRequest_ShouldReturnOkWithCourses()
    {
        // Arrange
        var courses = new List<CourseSummaryDto>
        {
            new CourseSummaryDto(
                Guid.NewGuid(),
                "Introduction to Autism EN",
                "Introduction to Autism AR",
                "Basic course about autism EN",
                "Basic course about autism AR",
                120,
                "thumbnail1.jpg",
                29.99m,
                "USD",
                "CRS-001",
                5,
                true)
        };

        var expectedResponse = new GetCoursesResponse(true, "Courses retrieved successfully", courses);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCoursesResponse>().Subject;
        
        returnValue.Courses.Should().HaveCount(1);
        returnValue.Courses.First().TitleEn.Should().Be("Introduction to Autism EN");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCoursesQuery>(q => q.ActiveOnly == true && q.SearchTerm == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCourses_WithSearchTerm_ShouldPassSearchTermToQuery()
    {
        // Arrange
        var searchTerm = "autism";
        var expectedResponse = new GetCoursesResponse(true, "No courses found", new List<CourseSummaryDto>());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCourses(activeOnly: false, searchTerm: searchTerm);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCoursesQuery>(q => q.ActiveOnly == false && q.SearchTerm == searchTerm),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCourseById_ExistingCourse_ShouldReturnOkWithCourse()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new CourseDto(
            courseId,
            "Advanced Autism Therapy EN",
            "Advanced Autism Therapy AR",
            "Advanced course description EN",
            "Advanced course description AR",
            240,
            "thumbnail2.jpg",
            49.99m,
            "USD",
            true,
            "CRS-002",
            8,
            DateTime.UtcNow,
            DateTime.UtcNow);

        var expectedResponse = new GetCourseByIdResponse(true, "Course retrieved successfully", course);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCourseById(courseId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCourseByIdResponse>().Subject;
        
        returnValue.Course.Should().NotBeNull();
        returnValue.Course!.Id.Should().Be(courseId);
        returnValue.Course.TitleEn.Should().Be("Advanced Autism Therapy EN");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCourseByIdQuery>(q => q.Id == courseId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCourseById_NonExistingCourse_ShouldReturnNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var expectedResponse = new GetCourseByIdResponse(
            false, 
            "Course not found", 
            null, 
            new Dictionary<string, string[]> { { "NotFound", new[] { "Course not found" } } });

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCourseById(courseId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
    }

    [Fact]
    public async Task EnrollInCourse_ValidRequest_ShouldReturnOkWithEnrollment()
    {
        // Arrange
        SetupAuthenticatedUser();
        var courseId = Guid.NewGuid();
        var request = new EnrollmentRequest { ValidityDays = 30 };

        var enrollment = new EnrollmentDto(
            Guid.NewGuid(),
            _testUserId,
            courseId,
            "Introduction to Autism EN",
            "Introduction to Autism AR",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            0,
            null,
            null,
            true,
            false,
            false,
            30);

        var expectedResponse = new EnrollUserResponse(true, "Enrollment successful", enrollment);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EnrollUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.EnrollInCourse(courseId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<EnrollUserResponse>().Subject;
        
        returnValue.Enrollment!.UserId.Should().Be(_testUserId);
        returnValue.Enrollment.CourseId.Should().Be(courseId);
        returnValue.Message.Should().Be("Enrollment successful");

        _mediatorMock.Verify(x => x.Send(
            It.Is<EnrollUserCommand>(c => 
                c.UserId == _testUserId &&
                c.CourseId == courseId &&
                c.ValidityDays == 30),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourse_Unauthorized_ShouldReturnUnauthorized()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new EnrollmentRequest { ValidityDays = 30 };

        // Act
        var result = await _controller.EnrollInCourse(courseId, request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.Value.Should().Be("User ID not found in token");

        _mediatorMock.Verify(x => x.Send(
            It.IsAny<EnrollUserCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserEnrollments_ValidRequest_ShouldReturnOkWithEnrollments()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollments = new List<EnrollmentDto>
        {
            new EnrollmentDto(
                Guid.NewGuid(),
                _testUserId,
                Guid.NewGuid(),
                "Course 1 EN",
                "Course 1 AR",
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(20),
                75,
                null,
                null,
                true,
                false,
                false,
                20)
        };

        var expectedResponse = new GetUserEnrollmentsResponse(true, "Enrollments retrieved successfully", enrollments);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserEnrollmentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUserEnrollments();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetUserEnrollmentsResponse>().Subject;
        
        returnValue.Enrollments.Should().HaveCount(1);
        returnValue.Enrollments.First().UserId.Should().Be(_testUserId);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetUserEnrollmentsQuery>(q => 
                q.UserId == _testUserId &&
                q.IncludeExpired == false &&
                q.IncludeCompleted == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEnrollmentProgress_ValidRequest_ShouldReturnOkWithProgress()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();
        var enrollment = new EnrollmentDto(
            enrollmentId,
            _testUserId,
            Guid.NewGuid(),
            "Course 1 EN",
            "Course 1 AR",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20),
            75,
            null,
            null,
            true,
            false,
            false,
            20);

        var moduleProgress = new List<ModuleProgressDto>
        {
            new ModuleProgressDto(
                Guid.NewGuid(),
                enrollmentId,
                Guid.NewGuid(),
                "Module 1 EN",
                "Module 1 AR",
                100,
                DateTime.UtcNow.AddDays(-1),
                3600,
                DateTime.UtcNow.AddDays(-10),
                DateTime.UtcNow.AddDays(-1))
        };

        var expectedResponse = new GetEnrollmentProgressResponse(
            true,
            "Progress retrieved successfully",
            enrollment,
            moduleProgress);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetEnrollmentProgressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetEnrollmentProgress(enrollmentId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetEnrollmentProgressResponse>().Subject;
        
        returnValue.Enrollment!.Id.Should().Be(enrollmentId);
        returnValue.Enrollment.ProgressPercentage.Should().Be(75);
        returnValue.ModuleProgress.Should().HaveCount(1);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetEnrollmentProgressQuery>(q => q.EnrollmentId == enrollmentId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProgress_ValidRequest_ShouldReturnOkWithUpdatedProgress()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var request = new UpdateProgressRequest
        {
            ModuleId = moduleId,
            ProgressPercentage = 85,
            TimeSpentMinutes = 45
        };

        var enrollment = new EnrollmentDto(
            enrollmentId,
            _testUserId,
            Guid.NewGuid(),
            "Course 1 EN",
            "Course 1 AR",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20),
            85,
            null,
            null,
            true,
            false,
            false,
            20);

        var expectedResponse = new UpdateProgressResponse(
            true,
            "Progress updated successfully",
            enrollment);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateProgressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateProgress(enrollmentId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<UpdateProgressResponse>().Subject;
        
        returnValue.Enrollment!.Id.Should().Be(enrollmentId);
        returnValue.Enrollment.ProgressPercentage.Should().Be(85);
        returnValue.Message.Should().Be("Progress updated successfully");

        _mediatorMock.Verify(x => x.Send(
            It.Is<UpdateProgressCommand>(c => 
                c.EnrollmentId == enrollmentId &&
                c.UserId == _testUserId &&
                c.ModuleId == moduleId &&
                c.ProgressPercentage == 85 &&
                c.TimeSpentMinutes == 45),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompletionStatus_ValidRequest_ShouldReturnOkWithStatus()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var completionStatus = new CourseCompletionStatusDto(
            enrollmentId,
            _testUserId,
            courseId,
            "Course Title EN",
            "Course Title AR",
            100,
            true,
            DateTime.UtcNow,
            "certificate-url.pdf",
            true,
            5,
            5,
            new List<ModuleCompletionDto>());

        var expectedResponse = new GetCourseCompletionStatusResponse(
            true,
            "Completion status retrieved successfully",
            completionStatus);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseCompletionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetCompletionStatus(enrollmentId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GetCourseCompletionStatusResponse>().Subject;
        
        returnValue.CompletionStatus!.EnrollmentId.Should().Be(enrollmentId);
        returnValue.CompletionStatus.IsCompleted.Should().BeTrue();
        returnValue.CompletionStatus.OverallProgressPercentage.Should().Be(100);

        _mediatorMock.Verify(x => x.Send(
            It.Is<GetCourseCompletionStatusQuery>(q => 
                q.EnrollmentId == enrollmentId &&
                q.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateCertificate_ValidRequest_ShouldReturnOkWithCertificate()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();

        var enrollment = new EnrollmentDto(
            enrollmentId,
            _testUserId,
            Guid.NewGuid(),
            "Course 1 EN",
            "Course 1 AR",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20),
            100,
            DateTime.UtcNow,
            "certificate-url.pdf",
            true,
            false,
            true,
            20);

        var expectedResponse = new GenerateCertificateResponse(
            true,
            "Certificate generated successfully",
            "certificate-url.pdf",
            enrollment);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GenerateCertificate(enrollmentId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<GenerateCertificateResponse>().Subject;
        
        returnValue.Enrollment!.Id.Should().Be(enrollmentId);
        returnValue.CertificateUrl.Should().Be("certificate-url.pdf");

        _mediatorMock.Verify(x => x.Send(
            It.Is<GenerateCertificateCommand>(c => 
                c.EnrollmentId == enrollmentId &&
                c.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadCertificate_ValidRequest_ShouldReturnFileResult()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };

        var expectedResponse = new DownloadCertificateResponse(
            true,
            "Certificate downloaded successfully",
            fileContent,
            "certificate.pdf",
            "application/pdf");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DownloadCertificate(enrollmentId);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.FileContents.Should().BeEquivalentTo(fileContent);
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("certificate.pdf");

        _mediatorMock.Verify(x => x.Send(
            It.Is<DownloadCertificateQuery>(q => 
                q.EnrollmentId == enrollmentId &&
                q.UserId == _testUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadCertificate_CertificateNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();

        var expectedResponse = new DownloadCertificateResponse(
            true,
            "Certificate not found",
            null,
            "",
            "");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DownloadCertificate(enrollmentId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Certificate file not found");
    }

    [Fact]
    public async Task ExtendEnrollment_ValidRequest_ShouldReturnOkWithExtendedEnrollment()
    {
        // Arrange
        SetupAuthenticatedUser();
        var enrollmentId = Guid.NewGuid();
        var request = new ExtendEnrollmentRequest { AdditionalDays = 15 };

        var enrollment = new EnrollmentDto(
            enrollmentId,
            _testUserId,
            Guid.NewGuid(),
            "Course 1 EN",
            "Course 1 AR",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(45),
            75,
            null,
            null,
            true,
            false,
            false,
            45);

        var expectedResponse = new ExtendEnrollmentResponse(
            true,
            "Enrollment extended successfully",
            enrollment);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ExtendEnrollmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExtendEnrollment(enrollmentId, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnValue = okResult.Value.Should().BeOfType<ExtendEnrollmentResponse>().Subject;
        
        returnValue.Enrollment!.Id.Should().Be(enrollmentId);
        returnValue.Enrollment.DaysRemaining.Should().Be(45);

        _mediatorMock.Verify(x => x.Send(
            It.Is<ExtendEnrollmentCommand>(c => 
                c.EnrollmentId == enrollmentId &&
                c.UserId == _testUserId &&
                c.AdditionalDays == 15),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCourses_ExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("An error occurred while retrieving courses");
    }

    private void SetupAuthenticatedUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "test"));

        _controller.ControllerContext.HttpContext.User = user;
    }
}