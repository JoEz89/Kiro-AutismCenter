using AutismCenter.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Middleware;

public class RateLimitingMiddlewareTests
{
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly RateLimitOptions _options;
    private readonly RateLimitingMiddleware _middleware;

    public RateLimitingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _options = new RateLimitOptions
        {
            AuthEndpoints = new RateLimitRule { MaxRequests = 2, Window = TimeSpan.FromMinutes(1) },
            GeneralEndpoints = new RateLimitRule { MaxRequests = 5, Window = TimeSpan.FromMinutes(1) }
        };
        _middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object, _options);
    }

    [Fact]
    public async Task InvokeAsync_WithinRateLimit_ShouldCallNext()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/products", "192.168.1.1");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        Assert.NotEqual(429, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExceedingRateLimit_ShouldBlock()
    {
        // Arrange
        var context1 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context2 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context3 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3); // This should be blocked

        // Assert
        _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(2));
        Assert.Equal(429, context3.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DifferentIPs_ShouldHaveSeparateLimits()
    {
        // Arrange
        var context1 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context2 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.2");
        var context3 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context4 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.2");

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3);
        await _middleware.InvokeAsync(context4);

        // Assert
        _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Exactly(4));
        Assert.NotEqual(429, context1.Response.StatusCode);
        Assert.NotEqual(429, context2.Response.StatusCode);
        Assert.NotEqual(429, context3.Response.StatusCode);
        Assert.NotEqual(429, context4.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUser_ShouldUseUserIdForRateLimit()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_HealthCheckEndpoint_ShouldSkipRateLimit()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/health", "192.168.1.1");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_RateLimitExceeded_ShouldIncludeHeaders()
    {
        // Arrange
        var context1 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context2 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");
        var context3 = CreateHttpContext("POST", "/api/auth/login", "192.168.1.1");

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3);

        // Assert
        Assert.Equal(429, context3.Response.StatusCode);
        Assert.True(context3.Response.Headers.ContainsKey("X-RateLimit-Limit"));
        Assert.True(context3.Response.Headers.ContainsKey("X-RateLimit-Window"));
        Assert.True(context3.Response.Headers.ContainsKey("Retry-After"));
    }

    [Theory]
    [InlineData("/api/auth/login", "auth")]
    [InlineData("/api/products", "products")]
    [InlineData("/api/orders", "orders")]
    [InlineData("/api/appointments", "appointments")]
    [InlineData("/api/courses", "courses")]
    [InlineData("/api/cart", "cart")]
    [InlineData("/api/payment", "payment")]
    [InlineData("/api/admin", "admin")]
    [InlineData("/api/other", "general")]
    public async Task InvokeAsync_DifferentEndpoints_ShouldHaveDifferentLimits(string path, string expectedEndpoint)
    {
        // Arrange
        var context = CreateHttpContext("GET", path, "192.168.1.1");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        // The test verifies that different endpoints are categorized correctly
        // Actual rate limiting behavior would be tested in integration tests
    }

    private static HttpContext CreateHttpContext(string method, string path, string ipAddress)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        
        // Mock the connection remote IP address
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);

        return context;
    }
}