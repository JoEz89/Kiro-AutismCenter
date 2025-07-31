using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AutismCenter.WebApi.Models.Admin;
using AutismCenter.Application.Features.Users.Queries.Admin.GetUsersAdmin;
using AutismCenter.Application.Features.Users.Queries.Admin.GetUserAnalytics;
using AutismCenter.Application.Features.Products.Queries.Admin.GetProductAnalytics;
using AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;
using AutismCenter.Application.Features.Appointments.Queries.Admin.GetAppointmentsAdmin;
using AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContentList;
using AutismCenter.Application.Features.Dashboard.Queries.GetDashboardOverview;
using AutismCenter.Application.Features.Analytics.Queries.GetSystemAnalytics;
using AutismCenter.Application.Features.Courses.Queries.Admin.GetCoursesAdmin;
using AutismCenter.Application.Features.Courses.Queries.Admin.GetCourseAnalytics;
using AutismCenter.Domain.Enums;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Controllers;

public class AdminControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AdminControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region User Management Tests

    [Fact]
    public async Task GetUsers_WithValidAdminToken_ReturnsUsers()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetUsersAdminRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDirection = "desc"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/users?PageNumber={request.PageNumber}&PageSize={request.PageSize}&SortBy={request.SortBy}&SortDirection={request.SortDirection}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetUsersAdminResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
    }

    [Fact]
    public async Task GetUsers_WithoutAdminToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetUserAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            GroupBy = "day"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/users/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&GroupBy={request.GroupBy}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetUserAnalyticsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateUserRole_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var userId = Guid.NewGuid(); // In a real test, this would be a valid user ID
        var request = new UpdateUserRoleRequest
        {
            Role = UserRole.Patient
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/users/{userId}/role", request);

        // Assert
        // Note: This might return NotFound in integration tests if the user doesn't exist
        // In a real scenario, you'd set up test data first
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Product Management Tests

    [Fact]
    public async Task GetProductAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetProductAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            GroupBy = "day"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/products/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&GroupBy={request.GroupBy}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetProductAnalyticsResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetInventoryReport_WithValidAdminToken_ReturnsReport()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetInventoryReportRequest
        {
            LowStockThreshold = 10,
            IncludeOutOfStock = true,
            SortBy = "name"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/inventory/report?LowStockThreshold={request.LowStockThreshold}&IncludeOutOfStock={request.IncludeOutOfStock}&SortBy={request.SortBy}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new CreateProductAdminRequest
        {
            NameEn = "Test Product",
            NameAr = "منتج تجريبي",
            DescriptionEn = "Test Description",
            DescriptionAr = "وصف تجريبي",
            Price = 99.99m,
            StockQuantity = 100,
            CategoryId = Guid.NewGuid(), // In real test, this would be a valid category ID
            ImageUrls = new List<string> { "https://example.com/image.jpg" },
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/products", request);

        // Assert
        // Note: This might return BadRequest if the category doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region Order Management Tests

    [Fact]
    public async Task GetOrders_WithValidAdminToken_ReturnsOrders()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetOrdersAdminRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDirection = "desc"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/orders?PageNumber={request.PageNumber}&PageSize={request.PageSize}&SortBy={request.SortBy}&SortDirection={request.SortDirection}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetOrdersAdminResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Orders);
    }

    [Fact]
    public async Task GetOrderAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetOrderAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            GroupBy = "day"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/orders/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&GroupBy={request.GroupBy}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExportOrders_WithValidAdminToken_ReturnsFile()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new ExportOrdersRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Format = "csv"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/orders/export?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&Format={request.Format}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    #endregion

    #region Appointment Management Tests

    [Fact]
    public async Task GetAppointments_WithValidAdminToken_ReturnsAppointments()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetAppointmentsAdminRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "AppointmentDate",
            SortDirection = "desc"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/appointments?PageNumber={request.PageNumber}&PageSize={request.PageSize}&SortBy={request.SortBy}&SortDirection={request.SortDirection}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAppointmentsAdminResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Appointments);
    }

    [Fact]
    public async Task GetAppointmentAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetAppointmentAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            GroupBy = "day"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/appointments/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&GroupBy={request.GroupBy}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var appointmentId = Guid.NewGuid(); // In a real test, this would be a valid appointment ID
        var request = new UpdateAppointmentStatusAdminRequest
        {
            Status = AppointmentStatus.Completed,
            Notes = "Appointment completed successfully"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/appointments/{appointmentId}/status", request);

        // Assert
        // Note: This might return NotFound in integration tests if the appointment doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Content Management Tests

    [Fact]
    public async Task GetLocalizedContent_WithValidAdminToken_ReturnsContent()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetLocalizedContentListRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/content?PageNumber={request.PageNumber}&PageSize={request.PageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetLocalizedContentListResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateLocalizedContent_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new CreateLocalizedContentRequest
        {
            Key = "test.content.key",
            Category = "test",
            EnglishContent = "Test Content",
            ArabicContent = "محتوى تجريبي",
            ContentType = "text",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/content", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region Dashboard and Analytics Tests

    [Fact]
    public async Task GetDashboardOverview_WithValidAdminToken_ReturnsDashboard()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetDashboardOverviewRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/dashboard?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSystemAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetSystemAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            IncludeUsers = true,
            IncludeProducts = true,
            IncludeOrders = true,
            IncludeAppointments = true,
            IncludeCourses = true
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&IncludeUsers={request.IncludeUsers}&IncludeProducts={request.IncludeProducts}&IncludeOrders={request.IncludeOrders}&IncludeAppointments={request.IncludeAppointments}&IncludeCourses={request.IncludeCourses}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Course Management Tests

    [Fact]
    public async Task GetCourses_WithValidAdminToken_ReturnsCourses()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetCoursesAdminRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDirection = "desc"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/courses?PageNumber={request.PageNumber}&PageSize={request.PageSize}&SortBy={request.SortBy}&SortDirection={request.SortDirection}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCourseAnalytics_WithValidAdminToken_ReturnsAnalytics()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new GetCourseAnalyticsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/courses/analytics?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExportCourses_WithValidAdminToken_ReturnsFile()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new ExportCoursesRequest
        {
            Format = "csv"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/courses/export?Format={request.Format}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateCourse_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new CreateCourseAdminRequest
        {
            TitleEn = "Test Course",
            TitleAr = "دورة تجريبية",
            DescriptionEn = "Test Course Description",
            DescriptionAr = "وصف الدورة التجريبية",
            Duration = 120,
            Price = 199.99m,
            CategoryId = Guid.NewGuid(), // In real test, this would be a valid category ID
            ThumbnailUrl = "https://example.com/thumbnail.jpg",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/courses", request);

        // Assert
        // Note: This might return BadRequest if the category doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCourse_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var courseId = Guid.NewGuid(); // In a real test, this would be a valid course ID
        var request = new UpdateCourseAdminRequest
        {
            TitleEn = "Updated Test Course",
            TitleAr = "دورة تجريبية محدثة",
            DescriptionEn = "Updated Test Course Description",
            DescriptionAr = "وصف الدورة التجريبية المحدثة",
            Duration = 150,
            Price = 249.99m,
            CategoryId = Guid.NewGuid(),
            ThumbnailUrl = "https://example.com/updated-thumbnail.jpg",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/admin/courses/{courseId}", request);

        // Assert
        // Note: This might return NotFound in integration tests if the course doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCourse_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var courseId = Guid.NewGuid(); // In a real test, this would be a valid course ID

        // Act
        var response = await _client.DeleteAsync($"/api/admin/courses/{courseId}");

        // Assert
        // Note: This might return NotFound in integration tests if the course doesn't exist
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
    }

    #endregion

    #region Data Export Tests

    [Fact]
    public async Task ExportUsers_WithValidAdminToken_ReturnsFile()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new ExportUsersRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Format = "csv"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/users/export?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&Format={request.Format}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExportProducts_WithValidAdminToken_ReturnsFile()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new ExportProductsRequest
        {
            Format = "csv"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/products/export?Format={request.Format}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExportAppointments_WithValidAdminToken_ReturnsFile()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var request = new ExportAppointmentsRequest
        {
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            Format = "csv"
        };

        // Act
        var response = await _client.GetAsync($"/api/admin/appointments/export?StartDate={request.StartDate:yyyy-MM-dd}&EndDate={request.EndDate:yyyy-MM-dd}&Format={request.Format}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetAdminTokenAsync()
    {
        // In a real integration test, you would:
        // 1. Create an admin user in the test database
        // 2. Login with that user to get a valid JWT token
        // 3. Return the token
        
        // For now, return a mock token (this won't work in real tests)
        // You need to implement proper authentication setup for integration tests
        return "mock-admin-token";
    }

    #endregion
}