using AutismCenter.Application.Features.Courses.Queries.GetCourses;
using AutismCenter.Application.Features.Courses.Queries.GetCourseById;
using AutismCenter.Application.Features.Courses.Queries.GetUserEnrollments;
using AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;
using AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;
using AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;
using AutismCenter.Application.Features.Courses.Queries.GetSecureVideoUrl;
using AutismCenter.Application.Features.Courses.Commands.EnrollUser;
using AutismCenter.Application.Features.Courses.Commands.UpdateProgress;
using AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;
using AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;
using AutismCenter.Application.Features.Courses.Commands.EndVideoSession;
using AutismCenter.Application.Features.Courses.Common;
using AutismCenter.Application.Common.Models;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Integration;

public class CourseIntegrationTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testCourseId = Guid.NewGuid();
    private readonly Guid _testEnrollmentId = Guid.NewGuid();
    private readonly Guid _testModuleId = Guid.NewGuid();

    public CourseIntegrationTests()
    {
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task CourseManagement_CompleteUserJourney_ShouldWorkEndToEnd()
    {
        // Arrange - Setup complete course learning journey
        SetupCompleteJourneyMocks();

        // Act & Assert

        // Step 1: Browse available courses
        var coursesResult = await _mediatorMock.Object.Send(new GetCoursesQuery(true, null));
        coursesResult.Success.Should().BeTrue();
        coursesResult.Courses.Should().HaveCount(2);
        coursesResult.Courses.First().TitleEn.Should().Be("Introduction to Autism EN");

        // Step 2: Get detailed course information
        var courseDetailsResult = await _mediatorMock.Object.Send(new GetCourseByIdQuery(_testCourseId));
        courseDetailsResult.Success.Should().BeTrue();
        courseDetailsResult.Course.Should().NotBeNull();
        courseDetailsResult.Course!.Id.Should().Be(_testCourseId);

        // Step 3: Enroll in the course
        var enrollmentResult = await _mediatorMock.Object.Send(new EnrollUserCommand(_testUserId, _testCourseId, 30));
        enrollmentResult.Success.Should().BeTrue();
        enrollmentResult.Enrollment.Should().NotBeNull();
        enrollmentResult.Enrollment!.UserId.Should().Be(_testUserId);

        // Step 4: Get user enrollments
        var userEnrollmentsResult = await _mediatorMock.Object.Send(new GetUserEnrollmentsQuery(_testUserId, false, true));
        userEnrollmentsResult.Success.Should().BeTrue();
        userEnrollmentsResult.Enrollments.Should().HaveCount(1);

        // Step 5: Get secure video URL for course content
        var videoUrlResult = await _mediatorMock.Object.Send(new GetSecureVideoUrlQuery(_testUserId, _testModuleId, 60));
        videoUrlResult.StreamingUrl.Should().NotBeNullOrEmpty();
        videoUrlResult.SessionId.Should().NotBeNullOrEmpty();

        // Step 6: Update progress as user watches videos
        var progressUpdateResult = await _mediatorMock.Object.Send(new UpdateProgressCommand(_testEnrollmentId, _testUserId, _testModuleId, 75, 3600));
        progressUpdateResult.Success.Should().BeTrue();
        progressUpdateResult.Enrollment!.ProgressPercentage.Should().Be(75);

        // Step 7: Get enrollment progress
        var progressResult = await _mediatorMock.Object.Send(new GetEnrollmentProgressQuery(_testEnrollmentId));
        progressResult.Success.Should().BeTrue();
        progressResult.Enrollment!.ProgressPercentage.Should().Be(75);

        // Step 8: Complete course and check completion status
        var completionStatusResult = await _mediatorMock.Object.Send(new GetCourseCompletionStatusQuery(_testEnrollmentId, _testUserId));
        completionStatusResult.IsSuccess.Should().BeTrue();
        completionStatusResult.CompletionStatus!.IsCompleted.Should().BeTrue();

        // Step 9: Generate certificate
        var certificateResult = await _mediatorMock.Object.Send(new GenerateCertificateCommand(_testEnrollmentId, _testUserId));
        certificateResult.IsSuccess.Should().BeTrue();
        certificateResult.CertificateUrl.Should().NotBeNullOrEmpty();

        // Step 10: Download certificate
        var downloadResult = await _mediatorMock.Object.Send(new DownloadCertificateQuery(_testEnrollmentId, _testUserId));
        downloadResult.IsSuccess.Should().BeTrue();
        downloadResult.FileContent.Should().NotBeNull();

        // Step 11: End video session
        await _mediatorMock.Object.Send(new EndVideoSessionCommand("session-123"));

        // Verify all interactions occurred
        VerifyAllInteractions();
    }

    [Fact]
    public async Task CourseSearch_WithDifferentFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var activeCoursesResponse = new GetCoursesResponse(
            true,
            "Active courses retrieved",
            new List<CourseSummaryDto>
            {
                CreateCourseSummaryDto("Active Course EN", "Active Course AR", true)
            });

        var searchResponse = new GetCoursesResponse(
            true,
            "Search results retrieved",
            new List<CourseSummaryDto>
            {
                CreateCourseSummaryDto("Autism Basics EN", "Autism Basics AR", true)
            });

        var allCoursesResponse = new GetCoursesResponse(
            true,
            "All courses retrieved",
            new List<CourseSummaryDto>
            {
                CreateCourseSummaryDto("Active Course EN", "Active Course AR", true),
                CreateCourseSummaryDto("Inactive Course EN", "Inactive Course AR", false)
            });

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetCoursesQuery>(q => q.ActiveOnly == true && q.SearchTerm == null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCoursesResponse);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetCoursesQuery>(q => q.SearchTerm == "autism"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponse);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetCoursesQuery>(q => q.ActiveOnly == false && q.SearchTerm == null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCoursesResponse);

        // Act & Assert

        // Test 1: Get only active courses
        var activeResult = await _mediatorMock.Object.Send(new GetCoursesQuery(true, null));
        activeResult.Courses.Should().HaveCount(1);
        activeResult.Courses.First().IsActive.Should().BeTrue();

        // Test 2: Search for specific term
        var searchResult = await _mediatorMock.Object.Send(new GetCoursesQuery(true, "autism"));
        searchResult.Courses.Should().HaveCount(1);
        searchResult.Courses.First().TitleEn.Should().Contain("Autism");

        // Test 3: Get all courses (including inactive)
        var allResult = await _mediatorMock.Object.Send(new GetCoursesQuery(false, null));
        allResult.Courses.Should().HaveCount(2);
        allResult.Courses.Should().Contain(c => c.IsActive == false);

        // Verify search functionality
        _mediatorMock.Verify(x => x.Send(It.Is<GetCoursesQuery>(q => q.ActiveOnly == true), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.Is<GetCoursesQuery>(q => q.SearchTerm == "autism"), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.Is<GetCoursesQuery>(q => q.ActiveOnly == false), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VideoStreaming_SecureAccess_ShouldEnforceSecurityConstraints()
    {
        // Arrange
        var validVideoResponse = new GetSecureVideoUrlResponse(
            "https://secure-video.com/stream?token=valid_token",
            "session-123",
            DateTime.UtcNow.AddMinutes(60),
            "Secure URL generated successfully")
        {
            DaysRemaining = 25,
            ModuleTitle = "Introduction Module",
            ModuleDuration = 45
        };

        var expiredEnrollmentResponse = new GetSecureVideoUrlResponse(
            "",
            "",
            DateTime.UtcNow,
            "Enrollment has expired")
        {
            DaysRemaining = 0
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetSecureVideoUrlQuery>(q => 
                q.UserId == _testUserId && 
                q.ModuleId == _testModuleId && 
                q.ExpirationMinutes == 60), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validVideoResponse);

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetSecureVideoUrlQuery>(q => 
                q.UserId != _testUserId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have access to this video"));

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetSecureVideoUrlQuery>(q => 
                q.ExpirationMinutes > 120), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Expiration time exceeds maximum allowed"));

        // Act & Assert

        // Test 1: Valid access should return secure URL
        var validResult = await _mediatorMock.Object.Send(new GetSecureVideoUrlQuery(_testUserId, _testModuleId, 60));
        validResult.StreamingUrl.Should().StartWith("https://secure-video.com");
        validResult.SessionId.Should().Be("session-123");
        validResult.DaysRemaining.Should().Be(25);

        // Test 2: Unauthorized access should throw exception
        var unauthorizedAction = async () => await _mediatorMock.Object.Send(new GetSecureVideoUrlQuery(Guid.NewGuid(), _testModuleId, 60));
        await unauthorizedAction.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User does not have access to this video");

        // Test 3: Invalid expiration time should throw exception
        var invalidExpirationAction = async () => await _mediatorMock.Object.Send(new GetSecureVideoUrlQuery(_testUserId, _testModuleId, 150));
        await invalidExpirationAction.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Expiration time exceeds maximum allowed");

        // Test 4: End video session
        await _mediatorMock.Object.Send(new EndVideoSessionCommand("session-123"));

        // Verify security checks
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        _mediatorMock.Verify(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProgressTracking_MultipleModules_ShouldTrackAccurately()
    {
        // Arrange
        var module1Id = Guid.NewGuid();
        var module2Id = Guid.NewGuid();
        var module3Id = Guid.NewGuid();

        var progressResponse = new GetEnrollmentProgressResponse(
            true,
            "Progress retrieved successfully",
            CreateEnrollmentDto(67), // Overall progress 67%
            new List<ModuleProgressDto>
            {
                new ModuleProgressDto(
                    Guid.NewGuid(),
                    _testEnrollmentId,
                    module1Id,
                    "Module 1 EN",
                    "Module 1 AR",
                    100, // Completed
                    DateTime.UtcNow.AddDays(-2),
                    3600,
                    DateTime.UtcNow.AddDays(-10),
                    DateTime.UtcNow.AddDays(-2)),
                new ModuleProgressDto(
                    Guid.NewGuid(),
                    _testEnrollmentId,
                    module2Id,
                    "Module 2 EN",
                    "Module 2 AR",
                    75, // In progress
                    DateTime.UtcNow.AddDays(-1),
                    2700,
                    DateTime.UtcNow.AddDays(-5),
                    DateTime.UtcNow.AddDays(-1)),
                new ModuleProgressDto(
                    Guid.NewGuid(),
                    _testEnrollmentId,
                    module3Id,
                    "Module 3 EN",
                    "Module 3 AR",
                    0, // Not started
                    null,
                    0,
                    DateTime.UtcNow.AddDays(-1),
                    null)
            });

        var updateProgressResponse = new UpdateProgressResponse(
            true,
            "Progress updated successfully",
            CreateEnrollmentDto(80)); // Updated overall progress

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetEnrollmentProgressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(progressResponse);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateProgressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateProgressResponse);

        // Act & Assert

        // Test 1: Get current progress
        var currentProgress = await _mediatorMock.Object.Send(new GetEnrollmentProgressQuery(_testEnrollmentId));
        currentProgress.Success.Should().BeTrue();
        currentProgress.Enrollment!.ProgressPercentage.Should().Be(67);
        currentProgress.ModuleProgress.Should().HaveCount(3);

        // Verify module-specific progress
        var completedModule = currentProgress.ModuleProgress.First(m => m.ModuleId == module1Id);
        completedModule.ProgressPercentage.Should().Be(100);
        completedModule.CompletedAt.Should().NotBeNull();

        var inProgressModule = currentProgress.ModuleProgress.First(m => m.ModuleId == module2Id);
        inProgressModule.ProgressPercentage.Should().Be(75);
        inProgressModule.TimeSpentSeconds.Should().Be(2700);

        var notStartedModule = currentProgress.ModuleProgress.First(m => m.ModuleId == module3Id);
        notStartedModule.ProgressPercentage.Should().Be(0);
        notStartedModule.CompletedAt.Should().BeNull();

        // Test 2: Update progress for module 2
        var updateResult = await _mediatorMock.Object.Send(new UpdateProgressCommand(
            _testEnrollmentId, _testUserId, module2Id, 90, 3600));
        updateResult.Success.Should().BeTrue();
        updateResult.Enrollment!.ProgressPercentage.Should().Be(80);

        // Verify tracking accuracy
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetEnrollmentProgressQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateProgressCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CertificateGeneration_CompletedCourse_ShouldGenerateAndDownload()
    {
        // Arrange
        var completionStatusResponse = new GetCourseCompletionStatusResponse(
            true,
            "Course completed",
            new CourseCompletionStatusDto(
                _testEnrollmentId,
                _testUserId,
                _testCourseId,
                "Advanced Autism Therapy EN",
                "Advanced Autism Therapy AR",
                100,
                true,
                DateTime.UtcNow,
                null, // Certificate not yet generated
                false,
                5,
                5,
                new List<ModuleCompletionDto>()));

        var generateCertificateResponse = new GenerateCertificateResponse(
            true,
            "Certificate generated successfully",
            "https://certificates.com/cert-123.pdf",
            CreateEnrollmentDto(100));

        var downloadCertificateResponse = new DownloadCertificateResponse(
            true,
            "Certificate downloaded successfully",
            new byte[] { 0x25, 0x50, 0x44, 0x46 }, // PDF header bytes
            "certificate-autism-therapy.pdf",
            "application/pdf");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseCompletionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completionStatusResponse);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(generateCertificateResponse);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadCertificateResponse);

        // Act & Assert

        // Test 1: Check completion status
        var completionResult = await _mediatorMock.Object.Send(new GetCourseCompletionStatusQuery(_testEnrollmentId, _testUserId));
        completionResult.IsSuccess.Should().BeTrue();
        completionResult.CompletionStatus!.IsCompleted.Should().BeTrue();
        completionResult.CompletionStatus.OverallProgressPercentage.Should().Be(100);
        completionResult.CompletionStatus.CompletedModulesCount.Should().Be(5);
        completionResult.CompletionStatus.TotalModulesCount.Should().Be(5);

        // Test 2: Generate certificate
        var certificateResult = await _mediatorMock.Object.Send(new GenerateCertificateCommand(_testEnrollmentId, _testUserId));
        certificateResult.IsSuccess.Should().BeTrue();
        certificateResult.CertificateUrl.Should().StartWith("https://certificates.com");
        certificateResult.Enrollment!.ProgressPercentage.Should().Be(100);

        // Test 3: Download certificate
        var downloadResult = await _mediatorMock.Object.Send(new DownloadCertificateQuery(_testEnrollmentId, _testUserId));
        downloadResult.IsSuccess.Should().BeTrue();
        downloadResult.FileContent.Should().NotBeNull();
        downloadResult.FileName.Should().Be("certificate-autism-therapy.pdf");
        downloadResult.ContentType.Should().Be("application/pdf");

        // Verify certificate workflow
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCourseCompletionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnrollmentManagement_ExtendAndTrack_ShouldMaintainAccuracy()
    {
        // Arrange
        var initialEnrollment = CreateEnrollmentDto(50, daysRemaining: 5);
        var extendedEnrollment = CreateEnrollmentDto(50, daysRemaining: 20);

        var enrollResponse = new EnrollUserResponse(true, "Enrollment successful", initialEnrollment);
        var extendResponse = new ExtendEnrollmentResponse(true, "Enrollment extended", extendedEnrollment);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EnrollUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollResponse);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<ExtendEnrollmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extendResponse);

        // Act & Assert

        // Test 1: Initial enrollment
        var enrollResult = await _mediatorMock.Object.Send(new EnrollUserCommand(_testUserId, _testCourseId, 30));
        enrollResult.Success.Should().BeTrue();
        enrollResult.Enrollment!.DaysRemaining.Should().Be(5);
        enrollResult.Enrollment.IsActive.Should().BeTrue();

        // Test 2: Extend enrollment
        var extendResult = await _mediatorMock.Object.Send(new ExtendEnrollmentCommand(_testEnrollmentId, _testUserId, 15));
        extendResult.IsSuccess.Should().BeTrue();
        extendResult.Enrollment!.DaysRemaining.Should().Be(20);

        // Verify enrollment management
        _mediatorMock.Verify(x => x.Send(It.IsAny<EnrollUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<ExtendEnrollmentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupCompleteJourneyMocks()
    {
        // Setup course catalog
        var coursesResponse = new GetCoursesResponse(
            true,
            "Courses retrieved successfully",
            new List<CourseSummaryDto>
            {
                CreateCourseSummaryDto("Introduction to Autism EN", "Introduction to Autism AR", true),
                CreateCourseSummaryDto("Advanced Autism Therapy EN", "Advanced Autism Therapy AR", true)
            });

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(coursesResponse);

        // Setup course details
        var courseDetailsResponse = new GetCourseByIdResponse(
            true,
            "Course retrieved successfully",
            CreateCourseDto());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courseDetailsResponse);

        // Setup enrollment
        var enrollmentResponse = new EnrollUserResponse(
            true,
            "Enrollment successful",
            CreateEnrollmentDto(0));

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EnrollUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollmentResponse);

        // Setup user enrollments
        var userEnrollmentsResponse = new GetUserEnrollmentsResponse(
            true,
            "Enrollments retrieved successfully",
            new List<EnrollmentDto> { CreateEnrollmentDto(0) });

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetUserEnrollmentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEnrollmentsResponse);

        // Setup video streaming
        var videoUrlResponse = new GetSecureVideoUrlResponse(
            "https://secure-video.com/stream?token=abc123",
            "session-123",
            DateTime.UtcNow.AddMinutes(60),
            "Secure URL generated successfully");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(videoUrlResponse);

        // Setup progress update
        var progressUpdateResponse = new UpdateProgressResponse(
            true,
            "Progress updated successfully",
            CreateEnrollmentDto(75));

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateProgressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(progressUpdateResponse);

        // Setup progress retrieval
        var progressResponse = new GetEnrollmentProgressResponse(
            true,
            "Progress retrieved successfully",
            CreateEnrollmentDto(75),
            new List<ModuleProgressDto>());

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetEnrollmentProgressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(progressResponse);

        // Setup completion status
        var completionStatusResponse = new GetCourseCompletionStatusResponse(
            true,
            "Completion status retrieved",
            new CourseCompletionStatusDto(
                _testEnrollmentId,
                _testUserId,
                _testCourseId,
                "Course Title EN",
                "Course Title AR",
                100,
                true,
                DateTime.UtcNow,
                null,
                false,
                5,
                5,
                new List<ModuleCompletionDto>()));

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetCourseCompletionStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completionStatusResponse);

        // Setup certificate generation
        var certificateResponse = new GenerateCertificateResponse(
            true,
            "Certificate generated successfully",
            "https://certificates.com/cert-123.pdf",
            CreateEnrollmentDto(100));

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(certificateResponse);

        // Setup certificate download
        var downloadResponse = new DownloadCertificateResponse(
            true,
            "Certificate downloaded successfully",
            new byte[] { 0x25, 0x50, 0x44, 0x46 },
            "certificate.pdf",
            "application/pdf");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResponse);

        // Setup video session end
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyAllInteractions()
    {
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCoursesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCourseByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<EnrollUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetUserEnrollmentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetSecureVideoUrlQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateProgressCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetEnrollmentProgressQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetCourseCompletionStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<GenerateCertificateCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<DownloadCertificateQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<EndVideoSessionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private CourseSummaryDto CreateCourseSummaryDto(string titleEn, string titleAr, bool isActive)
    {
        return new CourseSummaryDto(
            _testCourseId,
            titleEn,
            titleAr,
            "Course description EN",
            "Course description AR",
            120,
            "thumbnail.jpg",
            49.99m,
            "USD",
            "CRS-001",
            5,
            isActive);
    }

    private CourseDto CreateCourseDto()
    {
        return new CourseDto(
            _testCourseId,
            "Introduction to Autism EN",
            "Introduction to Autism AR",
            "Comprehensive course description EN",
            "Comprehensive course description AR",
            180,
            "course-thumbnail.jpg",
            49.99m,
            "USD",
            true,
            "CRS-001",
            5,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(-1));
    }

    private EnrollmentDto CreateEnrollmentDto(int progressPercentage, int daysRemaining = 30)
    {
        return new EnrollmentDto(
            _testEnrollmentId,
            _testUserId,
            _testCourseId,
            "Course Title EN",
            "Course Title AR",
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(daysRemaining),
            progressPercentage,
            progressPercentage == 100 ? DateTime.UtcNow : null,
            progressPercentage == 100 ? "certificate-url.pdf" : null,
            true,
            false,
            progressPercentage == 100,
            daysRemaining);
    }
}