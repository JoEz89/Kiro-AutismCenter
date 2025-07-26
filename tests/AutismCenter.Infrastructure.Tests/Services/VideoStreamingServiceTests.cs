using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Amazon.S3;
using Amazon.S3.Model;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Infrastructure.Services;
using Xunit;

namespace AutismCenter.Infrastructure.Tests.Services;

public class VideoStreamingServiceTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly Mock<IVideoAccessService> _mockVideoAccessService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<AwsS3VideoStreamingService>> _mockLogger;
    private readonly AwsS3VideoStreamingService _service;

    public VideoStreamingServiceTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _mockVideoAccessService = new Mock<IVideoAccessService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<AwsS3VideoStreamingService>>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["AWS:S3:VideoBucketName"]).Returns("test-video-bucket");
        _mockConfiguration.Setup(c => c["AWS:S3:VideoPrefix"]).Returns("course-videos/");

        _service = new AwsS3VideoStreamingService(
            _mockS3Client.Object,
            _mockVideoAccessService.Object,
            _mockConfiguration.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateSecureStreamingUrlAsync_ShouldReturnPresignedUrl_WhenValidRequest()
    {
        // Arrange
        var videoKey = "test-video.mp4";
        var userId = Guid.NewGuid();
        var expirationMinutes = 60;
        var expectedUrl = "https://test-bucket.s3.amazonaws.com/presigned-url";

        _mockS3Client.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _service.GenerateSecureStreamingUrlAsync(videoKey, userId, expirationMinutes);

        // Assert
        Assert.Equal(expectedUrl, result);
        _mockS3Client.Verify(s => s.GetPreSignedURLAsync(It.Is<GetPreSignedUrlRequest>(r =>
            r.BucketName == "test-video-bucket" &&
            r.Key == "course-videos/test-video.mp4" &&
            r.Verb == HttpVerb.GET &&
            r.ResponseHeaderOverrides.ContentDisposition == "inline"
        )), Times.Once);
    }

    [Fact]
    public async Task UploadVideoAsync_ShouldReturnVideoKey_WhenSuccessfulUpload()
    {
        // Arrange
        var fileName = "test-video.mp4";
        var contentType = "video/mp4";
        var videoStream = new MemoryStream();

        _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        // Act
        var result = await _service.UploadVideoAsync(videoStream, fileName, contentType);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(".mp4", result);
        _mockS3Client.Verify(s => s.PutObjectAsync(It.Is<PutObjectRequest>(r =>
            r.BucketName == "test-video-bucket" &&
            r.ContentType == contentType &&
            r.ServerSideEncryptionMethod == ServerSideEncryptionMethod.AES256
        ), default), Times.Once);
    }

    [Fact]
    public async Task DeleteVideoAsync_ShouldCallS3Delete_WhenValidVideoKey()
    {
        // Arrange
        var videoKey = "test-video.mp4";

        _mockS3Client.Setup(s => s.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ReturnsAsync(new DeleteObjectResponse());

        // Act
        await _service.DeleteVideoAsync(videoKey);

        // Assert
        _mockS3Client.Verify(s => s.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r =>
            r.BucketName == "test-video-bucket" &&
            r.Key == "course-videos/test-video.mp4"
        ), default), Times.Once);
    }

    [Fact]
    public async Task GetVideoMetadataAsync_ShouldReturnMetadata_WhenVideoExists()
    {
        // Arrange
        var videoKey = "test-video.mp4";
        var mockResponse = new GetObjectMetadataResponse
        {
            ContentLength = 1024000,
            LastModified = DateTime.UtcNow,
            Headers = { ContentType = "video/mp4" },
            Metadata = { ["original-filename"] = "original-video.mp4" }
        };

        _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _service.GetVideoMetadataAsync(videoKey);

        // Assert
        Assert.Equal(videoKey, result.VideoKey);
        Assert.Equal("original-video.mp4", result.FileName);
        Assert.Equal("video/mp4", result.ContentType);
        Assert.Equal(1024000, result.FileSizeBytes);
    }

    [Fact]
    public async Task GenerateSecureStreamingUrlAsync_ShouldThrowException_WhenS3Fails()
    {
        // Arrange
        var videoKey = "test-video.mp4";
        var userId = Guid.NewGuid();

        _mockS3Client.Setup(s => s.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ThrowsAsync(new AmazonS3Exception("S3 Error"));

        // Act & Assert
        await Assert.ThrowsAsync<AmazonS3Exception>(() =>
            _service.GenerateSecureStreamingUrlAsync(videoKey, userId));
    }
}