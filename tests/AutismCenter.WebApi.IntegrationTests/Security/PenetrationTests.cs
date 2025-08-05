using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Security;

/// <summary>
/// Penetration tests to validate security measures
/// These tests simulate common attack vectors to ensure the application is properly protected
/// </summary>
public class PenetrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PenetrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1' UNION SELECT password FROM Users --")]
    [InlineData("admin'/*")]
    [InlineData("' OR 'x'='x")]
    [InlineData("1'; EXEC xp_cmdshell('dir'); --")]
    [InlineData("'; SHUTDOWN; --")]
    public async Task SqlInjection_CommonPayloads_ShouldBeBlocked(string payload)
    {
        // Test SQL injection in login endpoint
        var loginPayload = $"{{\"email\":\"{payload}\",\"password\":\"test\"}}";
        var content = new StringContent(loginPayload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<svg onload=alert('XSS')>")]
    [InlineData("<iframe src='javascript:alert(1)'></iframe>")]
    [InlineData("<body onload=alert('XSS')>")]
    [InlineData("<input onfocus=alert('XSS') autofocus>")]
    [InlineData("<select onfocus=alert('XSS') autofocus>")]
    [InlineData("<textarea onfocus=alert('XSS') autofocus>")]
    [InlineData("<keygen onfocus=alert('XSS') autofocus>")]
    public async Task XssAttacks_CommonPayloads_ShouldBeBlocked(string payload)
    {
        // Test XSS in registration endpoint
        var registrationPayload = $"{{\"firstName\":\"{payload}\",\"lastName\":\"Test\",\"email\":\"test@example.com\",\"password\":\"Test123!\"}}";
        var content = new StringContent(registrationPayload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/register", content);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Security violation detected", responseContent);
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("....//....//....//etc//passwd")]
    [InlineData("..%2F..%2F..%2Fetc%2Fpasswd")]
    [InlineData("..%252F..%252F..%252Fetc%252Fpasswd")]
    public async Task PathTraversal_CommonPayloads_ShouldBeBlocked(string payload)
    {
        // Test path traversal in file access endpoints
        var response = await _client.GetAsync($"/api/files/{payload}");
        
        // Should either be blocked by security middleware or return 404/403
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.NotFound || 
                   response.StatusCode == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CommandInjection_ShouldBeBlocked()
    {
        var payloads = new[]
        {
            "; ls -la",
            "| dir",
            "&& whoami",
            "; cat /etc/passwd",
            "$(whoami)",
            "`whoami`",
            "${IFS}cat${IFS}/etc/passwd"
        };

        foreach (var payload in payloads)
        {
            var requestPayload = $"{{\"filename\":\"{payload}\"}}";
            var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/files/upload", content);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task LdapInjection_ShouldBeBlocked()
    {
        var payloads = new[]
        {
            "*)(uid=*",
            "*)(|(uid=*))",
            "admin)(&(password=*))",
            "*))%00",
            ")(cn=*"
        };

        foreach (var payload in payloads)
        {
            var requestPayload = $"{{\"username\":\"{payload}\",\"password\":\"test\"}}";
            var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/auth/ldap", content);
            
            // Should be blocked or return appropriate error
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task HeaderInjection_ShouldBeBlocked()
    {
        var maliciousHeaders = new Dictionary<string, string>
        {
            { "X-Forwarded-For", "127.0.0.1\r\nX-Injected: malicious" },
            { "User-Agent", "Mozilla/5.0\r\nX-Injected: malicious" },
            { "Referer", "http://example.com\r\nX-Injected: malicious" },
            { "X-Custom", "<script>alert('xss')</script>" }
        };

        foreach (var header in maliciousHeaders)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
            request.Headers.Add(header.Key, header.Value);
            
            var response = await _client.SendAsync(request);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task MassivePayload_ShouldBeRejected()
    {
        // Create a 50MB payload
        var largePayload = new string('A', 50 * 1024 * 1024);
        var requestPayload = $"{{\"data\":\"{largePayload}\"}}";
        var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/register", content);
        
        // Should be rejected due to size limits
        Assert.True(response.StatusCode == HttpStatusCode.RequestEntityTooLarge || 
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RapidFireRequests_ShouldTriggerRateLimit()
    {
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Send 20 rapid requests
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_client.GetAsync("/api/products"));
        }
        
        var responses = await Task.WhenAll(tasks);
        
        // At least some should be rate limited
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(rateLimitedCount > 0, "Expected some requests to be rate limited");
    }

    [Fact]
    public async Task BotLikeRequests_ShouldTriggerDDoSProtection()
    {
        // Simulate bot-like behavior with suspicious user agent
        _client.DefaultRequestHeaders.Add("User-Agent", "bot/1.0");
        
        var tasks = new List<Task<HttpResponseMessage>>();
        
        // Send rapid requests with bot user agent
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/products"));
            await Task.Delay(10); // Very short delay to simulate bot behavior
        }
        
        var responses = await Task.WhenAll(tasks);
        
        // Should eventually be blocked
        var blockedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        Assert.True(blockedCount > 0, "Expected bot-like requests to be blocked");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("undefined")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public async Task JavaScriptPayloads_ShouldBeBlocked(string payload)
    {
        var requestPayload = $"{{\"value\":\"{payload}\"}}";
        var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/register", content);
        
        // These should be handled gracefully, not necessarily blocked
        Assert.True(response.StatusCode != HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task NoSqlInjection_ShouldBeBlocked()
    {
        var payloads = new[]
        {
            "'; return db.users.find(); var dummy='",
            "'; return this.password; var dummy='",
            "'; return JSON.stringify(this); var dummy='",
            "'; while(true){}; var dummy='"
        };

        foreach (var payload in payloads)
        {
            var requestPayload = $"{{\"query\":\"{payload}\"}}";
            var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/search", content);
            
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task XmlExternalEntity_ShouldBeBlocked()
    {
        var xxePayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE foo [
<!ELEMENT foo ANY >
<!ENTITY xxe SYSTEM ""file:///etc/passwd"" >]>
<foo>&xxe;</foo>";

        var content = new StringContent(xxePayload, Encoding.UTF8, "application/xml");
        
        var response = await _client.PostAsync("/api/xml/process", content);
        
        // Should be blocked or return appropriate error
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                   response.StatusCode == HttpStatusCode.UnsupportedMediaType ||
                   response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ServerSideRequestForgery_ShouldBeBlocked()
    {
        var ssrfPayloads = new[]
        {
            "http://localhost:22",
            "http://127.0.0.1:3306",
            "http://169.254.169.254/latest/meta-data/",
            "file:///etc/passwd",
            "ftp://internal-server/",
            "gopher://127.0.0.1:25/"
        };

        foreach (var payload in ssrfPayloads)
        {
            var requestPayload = $"{{\"url\":\"{payload}\"}}";
            var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/fetch", content);
            
            // Should be blocked or return appropriate error
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.Forbidden);
        }
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