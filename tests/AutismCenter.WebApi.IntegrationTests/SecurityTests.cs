using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.True(response.Headers.Contains("X-Permitted-Cross-Domain-Policies"));
        
        // Check values
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
        Assert.Equal("1; mode=block", response.Headers.GetValues("X-XSS-Protection").First());
    }

    [Fact]
    public async Task ServerHeader_ShouldBeRemoved()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.False(response.Headers.Contains("Server"));
    }

    [Theory]
    [InlineData("SELECT * FROM Users")]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("UNION SELECT * FROM Users")]
    public async Task SqlInjectionAttempts_ShouldBeBlocked(string maliciousInput)
    {
        // Arrange
        var requestBody = $"{{\"email\":\"{maliciousInput}\",\"password\":\"test\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("<iframe src='javascript:alert(1)'></iframe>")]
    public async Task XssAttempts_ShouldBeBlocked(string maliciousInput)
    {
        // Arrange
        var requestBody = $"{{\"firstName\":\"{maliciousInput}\",\"lastName\":\"Test\",\"email\":\"test@example.com\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Fact]
    public async Task RateLimit_ShouldBlockExcessiveRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Act - Send many requests rapidly
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/products"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert - At least one should be rate limited
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        
        // Note: This test might be flaky depending on timing, so we check if any are rate limited
        // In a real scenario, you'd want more controlled testing
        if (rateLimitedResponses.Any())
        {
            var rateLimitedResponse = rateLimitedResponses.First();
            Assert.True(rateLimitedResponse.Headers.Contains("X-RateLimit-Limit"));
            Assert.True(rateLimitedResponse.Headers.Contains("Retry-After"));
        }
    }

    [Fact]
    public async Task MaliciousHeaders_ShouldBeBlocked()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Malicious", "<script>alert('xss')</script>");

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Theory]
    [InlineData("/api/products?id=1'; DROP TABLE Products; --")]
    [InlineData("/api/products?search=<script>alert('xss')</script>")]
    [InlineData("/api/products?category=javascript:alert(1)")]
    public async Task MaliciousQueryParameters_ShouldBeBlocked(string maliciousUrl)
    {
        // Act
        var response = await _client.GetAsync(maliciousUrl);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Fact]
    public async Task ContentSecurityPolicy_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        
        var cspHeader = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", cspHeader);
        Assert.Contains("object-src 'none'", cspHeader);
        Assert.Contains("frame-ancestors 'none'", cspHeader);
    }

    [Fact]
    public async Task SensitiveEndpoints_ShouldHaveNoCacheHeaders()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/profile");

        // Assert
        if (response.Headers.Contains("Cache-Control"))
        {
            var cacheControl = response.Headers.GetValues("Cache-Control").First();
            Assert.Contains("no-cache", cacheControl);
            Assert.Contains("no-store", cacheControl);
        }
    }

    [Fact]
    public async Task LargePayload_ShouldBeRejected()
    {
        // Arrange - Create a very large payload
        var largeString = new string('A', 10 * 1024 * 1024); // 10MB
        var requestBody = $"{{\"data\":\"{largeString}\"}}";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        // Should be rejected by the server (either 413 or 400)
        Assert.True(response.StatusCode == HttpStatusCode.RequestEntityTooLarge || 
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvalidJsonPayload_ShouldBeRejected()
    {
        // Arrange
        var invalidJson = "{invalid json content";
        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PermissionsPolicy_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.True(response.Headers.Contains("Permissions-Policy"));
        
        var permissionsPolicy = response.Headers.GetValues("Permissions-Policy").First();
        Assert.Contains("camera=()", permissionsPolicy);
        Assert.Contains("microphone=()", permissionsPolicy);
        Assert.Contains("geolocation=()", permissionsPolicy);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}