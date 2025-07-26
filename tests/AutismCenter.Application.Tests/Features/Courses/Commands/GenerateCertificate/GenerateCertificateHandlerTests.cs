using Xunit;
using Moq;
using AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;

namespace AutismCenter.Application.Tests.Features.Courses.Commands.GenerateCertificate;

public class GenerateCertificateHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<ICertificateService> _certificateServiceMock;
    private readonly GenerateCertificateHandler _handler;

    public GenerateCertificateHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _certificateServiceMock = new Mock<ICertificateService>();
        _handler = new GenerateCertificateHandler(_enrollmentRepositoryMock.Object, _certificateServiceMock.Object);
    }

    [Fact]
    public async Task Handle_EnrollmentNotFound_ReturnsFailureResponse()
    {
        // Arrange
        var command = new GenerateCertificateCommand(Guid.NewGuid());
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enrollment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Enrollment not found", result.Message);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains(nameof(command.EnrollmentId), result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_EnrollmentNotCompleted_ReturnsFailureResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        var command = new GenerateCertificateCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Certificate can only be generated for completed courses", result.Message);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Completion", result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task Handle_CertificateAlreadyExists_ReturnsSuccessWithExistingCertificate()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var existingCertificateUrl = "https://example.com/certificate.pdf";
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted();
        enrollment.GenerateCertificate(existingCertificateUrl);
        
        var command = new GenerateCertificateCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Certificate already exists", result.Message);
        Assert.Equal(existingCertificateUrl, result.CertificateUrl);
        Assert.NotNull(result.Enrollment);
    }

    [Fact]
    public async Task Handle_ValidCompletedEnrollment_GeneratesCertificateSuccessfully()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var certificateUrl = "https://example.com/new-certificate.pdf";
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted();
        
        var command = new GenerateCertificateCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _certificateServiceMock.Setup(x => x.GenerateCertificateAsync(enrollment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(certificateUrl);

        _enrollmentRepositoryMock.Setup(x => x.UpdateAsync(enrollment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Certificate generated successfully", result.Message);
        Assert.Equal(certificateUrl, result.CertificateUrl);
        Assert.NotNull(result.Enrollment);
        
        // Verify certificate service was called
        _certificateServiceMock.Verify(x => x.GenerateCertificateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify enrollment was updated
        _enrollmentRepositoryMock.Verify(x => x.UpdateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CertificateServiceThrowsException_ReturnsFailureResponse()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted();
        
        var command = new GenerateCertificateCommand(enrollmentId);

        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(command.EnrollmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _certificateServiceMock.Setup(x => x.GenerateCertificateAsync(enrollment, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Certificate generation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An unexpected error occurred while generating certificate", result.Message);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("Error", result.ValidationErrors.Keys);
    }
}