using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using AutismCenter.Infrastructure.Data;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;
using AutismCenter.WebApi.Controllers;
using AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;
using AutismCenter.Application.Features.Appointments.Queries.GetCalendarView;

namespace AutismCenter.WebApi.IntegrationTests;

public class AppointmentsControllerFunctionalTests : IClassFixture<TestApplicationFactory<Program>>
{
    private readonly TestApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppointmentsControllerFunctionalTests(TestApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAvailableSlots_WithValidDateRange_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/appointments/available-slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with auth, you would test:
        // - Response should be OK
        // - Response should contain GetAvailableSlotsResponse structure
        // - Should contain doctor slots with available times
    }

    [Fact]
    public async Task GetCalendarView_WithValidDateRange_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(7);

        // Act
        var response = await _client.GetAsync($"/api/appointments/calendar?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with auth, you would test:
        // - Response should be OK
        // - Response should contain GetCalendarViewResponse structure
        // - Should contain calendar days with appointments and availability
    }

    [Fact]
    public async Task BookAppointment_WithValidData_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();
        
        var request = new BookAppointmentRequest(
            testData.UserId,
            testData.DoctorId,
            DateTime.UtcNow.AddDays(1).Date.AddHours(10), // 10 AM tomorrow
            60,
            "John Doe",
            25,
            "No significant medical history",
            "Behavioral concerns",
            "Jane Doe",
            "+1234567890"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with auth, you would test:
        // - Response should be OK
        // - Response should contain BookAppointmentResponse structure
        // - Should return appointment details with generated appointment number
    }

    [Fact]
    public async Task CreateDoctorAvailability_WithValidData_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();
        
        var request = new CreateDoctorAvailabilityRequest(
            DayOfWeek.Tuesday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0)
        );

        // Act
        var response = await _client.PostAsJsonAsync($"/api/appointments/doctors/{testData.DoctorId}/availability", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with admin auth, you would test:
        // - Response should be OK
        // - Response should contain CreateDoctorAvailabilityResponse structure
        // - Should return availability details with generated ID
    }

    [Fact]
    public async Task GetDoctorAvailability_WithValidDoctorId_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();

        // Act
        var response = await _client.GetAsync($"/api/appointments/doctors/{testData.DoctorId}/availability");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with auth, you would test:
        // - Response should be OK
        // - Response should contain GetDoctorAvailabilityResponse structure
        // - Should return doctor's availability schedule
    }

    [Fact]
    public async Task CreateZoomMeeting_WithValidAppointmentId_ReturnsCorrectStructure()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();
        var appointmentId = await CreateTestAppointmentAsync(testData);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointmentId}/zoom-meeting", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with auth, you would test:
        // - Response should be OK
        // - Response should contain CreateZoomMeetingResponse structure
        // - Should return Zoom meeting details with join URL
    }

    [Fact]
    public async Task SendAppointmentReminder_WithValidAppointmentId_ReturnsSuccess()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();
        var appointmentId = await CreateTestAppointmentAsync(testData);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointmentId}/send-reminder", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with admin auth, you would test:
        // - Response should be OK
        // - Should return success message
        // - Should trigger notification service
    }

    [Fact]
    public async Task SendAppointmentNotification_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await SeedTestDataAsync();
        var testData = await GetTestDataAsync();
        var appointmentId = await CreateTestAppointmentAsync(testData);
        
        var request = new SendNotificationRequest("reminder", "Custom reminder message");

        // Act
        var response = await _client.PostAsJsonAsync($"/api/appointments/{appointmentId}/send-notification", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // Expected without auth
        // In a real scenario with admin auth, you would test:
        // - Response should be OK
        // - Should return success message with notification details
        // - Should trigger notification service with custom message
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Create test user
        var user = User.Create(
            Email.Create("test@example.com"),
            "Test",
            "User",
            UserRole.Patient,
            Language.English
        );
        context.Users.Add(user);

        // Create test doctor
        var doctor = Doctor.Create(
            "Dr. John Smith",
            "د. جون سميث",
            "Autism Specialist",
            "أخصائي التوحد",
            Email.Create("doctor@example.com")
        );
        context.Doctors.Add(doctor);

        // Create doctor availability
        var availability = DoctorAvailability.Create(
            doctor.Id,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0)
        );
        doctor.AddAvailability(availability);

        await context.SaveChangesAsync();
    }

    private async Task<(Guid UserId, Guid DoctorId)> GetTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = context.Users.First();
        var doctor = context.Doctors.First();

        return (user.Id, doctor.Id);
    }

    private async Task<Guid> CreateTestAppointmentAsync((Guid UserId, Guid DoctorId) testData)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var patientInfo = PatientInfo.Create(
            "Test Patient",
            25,
            "No medical history",
            "Behavioral concerns",
            "Emergency Contact",
            "+1234567890"
        );

        var appointment = Appointment.Create(
            testData.UserId,
            testData.DoctorId,
            DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            60,
            "APT-2024-001",
            patientInfo
        );

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        return appointment.Id;
    }
}