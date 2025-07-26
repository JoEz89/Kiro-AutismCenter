using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Domain.Tests.Services;

public class AppointmentZoomServiceTests
{
    private readonly Mock<IZoomService> _zoomServiceMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly AppointmentZoomService _service;

    public AppointmentZoomServiceTests()
    {
        _zoomServiceMock = new Mock<IZoomService>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _service = new AppointmentZoomService(_zoomServiceMock.Object, _appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateMeetingForAppointmentAsync_ValidAppointment_ShouldCreateMeetingAndUpdateAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        var zoomMeeting = new ZoomMeeting(
            "123456789",
            "Test Meeting",
            "https://zoom.us/j/123456789",
            "https://zoom.us/s/123456789",
            appointmentDate,
            60,
            "password123",
            ZoomMeetingStatus.Waiting);

        _zoomServiceMock.Setup(x => x.CreateMeetingAsync(It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(zoomMeeting);

        _appointmentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateMeetingForAppointmentAsync(appointment);

        // Assert
        result.Should().Be("https://zoom.us/j/123456789");
        appointment.ZoomMeetingId.Should().Be("123456789");
        appointment.ZoomJoinUrl.Should().Be("https://zoom.us/j/123456789");
        appointment.HasZoomMeeting().Should().BeTrue();

        _zoomServiceMock.Verify(x => x.CreateMeetingAsync(
            It.Is<ZoomMeetingRequest>(r => 
                r.Topic.Contains("John Doe") && 
                r.StartTime == appointmentDate && 
                r.DurationInMinutes == 60),
            It.IsAny<CancellationToken>()), Times.Once);

        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMeetingForAppointmentAsync_AppointmentAlreadyHasMeeting_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);
        appointment.SetZoomMeeting("existing-meeting", "https://zoom.us/j/existing");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateMeetingForAppointmentAsync(appointment));

        exception.Message.Should().Contain("already has a Zoom meeting");
    }

    [Fact]
    public async Task UpdateMeetingForAppointmentAsync_ValidAppointment_ShouldUpdateMeeting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);
        appointment.SetZoomMeeting("123456789", "https://zoom.us/j/123456789");

        _zoomServiceMock.Setup(x => x.UpdateMeetingAsync(It.IsAny<string>(), It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateMeetingForAppointmentAsync(appointment);

        // Assert
        _zoomServiceMock.Verify(x => x.UpdateMeetingAsync(
            "123456789",
            It.Is<ZoomMeetingRequest>(r => 
                r.Topic.Contains("John Doe") && 
                r.StartTime == appointmentDate),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMeetingForAppointmentAsync_AppointmentWithoutMeeting_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateMeetingForAppointmentAsync(appointment));

        exception.Message.Should().Contain("does not have a Zoom meeting");
    }

    [Fact]
    public async Task DeleteMeetingForAppointmentAsync_ValidAppointment_ShouldDeleteMeetingAndClearAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);
        appointment.SetZoomMeeting("123456789", "https://zoom.us/j/123456789");

        _zoomServiceMock.Setup(x => x.DeleteMeetingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteMeetingForAppointmentAsync(appointment);

        // Assert
        appointment.HasZoomMeeting().Should().BeFalse();
        
        _zoomServiceMock.Verify(x => x.DeleteMeetingAsync("123456789", It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMeetingForAppointmentAsync_AppointmentWithoutMeeting_ShouldDoNothing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        // Act
        await _service.DeleteMeetingForAppointmentAsync(appointment);

        // Assert
        _zoomServiceMock.Verify(x => x.DeleteMeetingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAppointmentRescheduledAsync_AppointmentWithoutMeeting_ShouldCreateNewMeeting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        var zoomMeeting = new ZoomMeeting(
            "123456789",
            "Test Meeting",
            "https://zoom.us/j/123456789",
            "https://zoom.us/s/123456789",
            appointmentDate,
            60,
            "password123",
            ZoomMeetingStatus.Waiting);

        _zoomServiceMock.Setup(x => x.CreateMeetingAsync(It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(zoomMeeting);

        _appointmentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.HandleAppointmentRescheduledAsync(appointment);

        // Assert
        appointment.HasZoomMeeting().Should().BeTrue();
        _zoomServiceMock.Verify(x => x.CreateMeetingAsync(It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAppointmentRescheduledAsync_AppointmentWithMeeting_ShouldUpdateExistingMeeting()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe", 25);
        
        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);
        appointment.SetZoomMeeting("123456789", "https://zoom.us/j/123456789");

        _zoomServiceMock.Setup(x => x.UpdateMeetingAsync(It.IsAny<string>(), It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.HandleAppointmentRescheduledAsync(appointment);

        // Assert
        _zoomServiceMock.Verify(x => x.UpdateMeetingAsync("123456789", It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _zoomServiceMock.Verify(x => x.CreateMeetingAsync(It.IsAny<ZoomMeetingRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}