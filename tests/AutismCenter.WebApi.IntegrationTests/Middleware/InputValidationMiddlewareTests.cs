using AutismCenter.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Middleware;

public class InputValidationMiddlewareTests
{
    private readonly Mock<ILogger<InputValidationMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly InputValidationMiddleware _middleware;

    public InputValidationMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<InputValidationMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new InputValidationMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/products", null);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithHealthCheckPath_ShouldSkipValidation()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/health", null);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData("SELECT * FROM Users")]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("UNION SELECT password FROM Users")]
    public async Task InvokeAsync_WithSqlInjectionInBody_ShouldBlock(string maliciousInput)
    {
        // Arrange
        var requestBody = $"{{\"email\":\"{maliciousInput}\"}}";
        var context = CreateHttpContext("POST", "/api/auth/login", requestBody);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("onload=alert('xss')")]
    public async Task InvokeAsync_WithXssInBody_ShouldBlock(string maliciousInput)
    {
        // Arrange
        var requestBody = $"{{\"content\":\"{maliciousInput}\"}}";
        var context = CreateHttpContext("POST", "/api/content", requestBody);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMaliciousQueryParameter_ShouldBlock()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/products", null);
        context.Request.QueryString = new QueryString("?search=<script>alert('xss')</script>");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMaliciousHeader_ShouldBlock()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/products", null);
        context.Request.Headers.Add("X-Custom", "<script>alert('xss')</script>");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithValidJsonContent_ShouldSanitize()
    {
        // Arrange
        var requestBody = "{\"name\":\"<b>Test</b>\",\"description\":\"Valid content\"}";
        var context = CreateHttpContext("POST", "/api/products", requestBody);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        
        // Verify the content was sanitized (HTML tags removed)
        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body);
        var sanitizedContent = await reader.ReadToEndAsync();
        Assert.DoesNotContain("<b>", sanitizedContent);
        Assert.Contains("Test", sanitizedContent);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ShouldBlock()
    {
        // Arrange
        var invalidJson = "{invalid json content";
        var context = CreateHttpContext("POST", "/api/products", invalidJson);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Never);
        Assert.Equal(400, context.Response.StatusCode);
    }

    private static HttpContext CreateHttpContext(string method, string path, string? body)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.ContentType = "application/json";
        context.Response.Body = new MemoryStream();

        if (!string.IsNullOrEmpty(body))
        {
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bodyBytes);
            context.Request.ContentLength = bodyBytes.Length;
        }

        return context;
    }
}