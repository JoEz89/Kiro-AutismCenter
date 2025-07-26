using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutismCenter.Infrastructure.Services;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using System.Text;

namespace AutismCenter.Infrastructure.Tests.Services;

public class CertificateServiceTests : IDisposable
{
    private readonly Mock<ILogger<CertificateService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly CertificateService _certificateService;
    private readonly string _testCertificatePath;

    public CertificateServiceTests()
    {
        _loggerMock = new Mock<ILogger<CertificateService>>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup test certificate directory
        _testCertificatePath = Path.Combine(Path.GetTempPath(), "test_certificates", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testCertificatePath);
        
        _configurationMock.Setup(x => x["CertificateStorage:Path"])
            .Returns(_testCertificatePath);
        
        _certificateService = new CertificateService(_loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task GenerateCertificateAsync_ValidEnrollment_ReturnsValidUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted();

        // Create a mock user and course
        var user = User.Create(
            Email.Create("test@example.com"),
            "John",
            "Doe"
        );
        
        var course = Course.Create(
            "Test Course",
            "دورة تجريبية",
            "Test Description",
            "وصف تجريبي",
            120,
            Money.Create(100, "USD"),
            "CRS-001"
        );

        // Set user and course using reflection
        var userProperty = typeof(Enrollment).GetProperty("User");
        userProperty?.SetValue(enrollment, user);
        
        var courseProperty = typeof(Enrollment).GetProperty("Course");
        courseProperty?.SetValue(enrollment, course);

        // Act
        var result = await _certificateService.GenerateCertificateAsync(enrollment);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("/certificates/", result);
        Assert.Contains(enrollment.Id.ToString(), result);
        
        // Verify file was created
        var fileName = Path.GetFileName(result);
        var filePath = Path.Combine(_testCertificatePath, fileName);
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task GenerateCertificatePdfAsync_ValidEnrollment_ReturnsValidPdfData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var enrollment = Enrollment.CreateEnrollment(userId, courseId);
        enrollment.MarkAsCompleted();

        // Create a mock user and course
        var user = User.Create(
            Email.Create("test@example.com"),
            "John",
            "Doe"
        );
        
        var course = Course.Create(
            "Test Course",
            "دورة تجريبية",
            "Test Description",
            "وصف تجريبي",
            120,
            Money.Create(100, "USD"),
            "CRS-001"
        );

        // Set user and course using reflection
        var userProperty = typeof(Enrollment).GetProperty("User");
        userProperty?.SetValue(enrollment, user);
        
        var courseProperty = typeof(Enrollment).GetProperty("Course");
        courseProperty?.SetValue(enrollment, course);

        // Act
        var result = await _certificateService.GenerateCertificatePdfAsync(enrollment);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // Verify content contains expected information
        var content = Encoding.UTF8.GetString(result);
        Assert.Contains("CERTIFICATE OF COMPLETION", content);
        Assert.Contains("John Doe", content);
        Assert.Contains("Test Course", content);
        Assert.Contains(enrollment.Id.ToString(), content);
    }

    [Fact]
    public async Task StoreCertificateAsync_ValidData_StoresFileAndReturnsUrl()
    {
        // Arrange
        var certificateData = Encoding.UTF8.GetBytes("Test certificate content");
        var fileName = "test_certificate.pdf";

        // Act
        var result = await _certificateService.StoreCertificateAsync(certificateData, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal($"/certificates/{fileName}", result);
        
        // Verify file was created
        var filePath = Path.Combine(_testCertificatePath, fileName);
        Assert.True(File.Exists(filePath));
        
        // Verify file content
        var storedContent = await File.ReadAllBytesAsync(filePath);
        Assert.Equal(certificateData, storedContent);
    }

    [Fact]
    public async Task DeleteCertificateAsync_ExistingFile_DeletesFileAndReturnsTrue()
    {
        // Arrange
        var fileName = "test_certificate_to_delete.pdf";
        var filePath = Path.Combine(_testCertificatePath, fileName);
        var certificateUrl = $"/certificates/{fileName}";
        
        // Create test file
        await File.WriteAllTextAsync(filePath, "Test content");
        Assert.True(File.Exists(filePath));

        // Act
        var result = await _certificateService.DeleteCertificateAsync(certificateUrl);

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteCertificateAsync_NonExistingFile_ReturnsFalse()
    {
        // Arrange
        var certificateUrl = "/certificates/non_existing_file.pdf";

        // Act
        var result = await _certificateService.DeleteCertificateAsync(certificateUrl);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteCertificateAsync_EmptyUrl_ReturnsFalse()
    {
        // Arrange
        var certificateUrl = string.Empty;

        // Act
        var result = await _certificateService.DeleteCertificateAsync(certificateUrl);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testCertificatePath))
        {
            Directory.Delete(_testCertificatePath, true);
        }
    }
}