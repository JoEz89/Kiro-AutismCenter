using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Domain.Tests.Services;

public class AppointmentSlotServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly AppointmentSlotService _service;

    public AppointmentSlotServiceTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _service = new AppointmentSlotService(_appointmentRepositoryMock.Object, _doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task IsSlotAvailableAsync_ValidSlot_ShouldReturnTrue()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var availability = DoctorAvailability.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        doctor.AddAvailability(availability);

        var slotTime = GetNextMondayAt9AM();

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentRepositoryMock.Setup(x => x.HasConflictingAppointmentAsync(
            doctorId, slotTime, slotTime.AddMinutes(60), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsSlotAvailableAsync(doctorId, slotTime, 60);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlotAvailableAsync_DoctorNotFound_ShouldReturnFalse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var slotTime = GetNextMondayAt9AM();

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var result = await _service.IsSlotAvailableAsync(doctorId, slotTime, 60);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlotAvailableAsync_DoctorInactive_ShouldReturnFalse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        doctor.Deactivate();

        var slotTime = GetNextMondayAt9AM();

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _service.IsSlotAvailableAsync(doctorId, slotTime, 60);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlotAvailableAsync_OutsideAvailability_ShouldReturnFalse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        // Doctor available 9-17, but requesting slot at 8 AM
        var availability = DoctorAvailability.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        doctor.AddAvailability(availability);

        var slotTime = GetNextMondayAt9AM().AddHours(-1); // 8 AM

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _service.IsSlotAvailableAsync(doctorId, slotTime, 60);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlotAvailableAsync_ConflictingAppointment_ShouldReturnFalse()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var availability = DoctorAvailability.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        doctor.AddAvailability(availability);

        var slotTime = GetNextMondayAt9AM();

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentRepositoryMock.Setup(x => x.HasConflictingAppointmentAsync(
            doctorId, slotTime, slotTime.AddMinutes(60), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsSlotAvailableAsync(doctorId, slotTime, 60);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAppointmentSlotAsync_PastTime_ShouldThrowException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var pastTime = DateTime.UtcNow.AddMinutes(-30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ValidateAppointmentSlotAsync(doctorId, pastTime, 60));

        exception.Message.Should().Contain("at least 30 minutes in advance");
    }

    [Fact]
    public async Task ValidateAppointmentSlotAsync_InvalidDuration_ShouldThrowException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var futureTime = DateTime.UtcNow.AddHours(2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ValidateAppointmentSlotAsync(doctorId, futureTime, -30));

        exception.Message.Should().Contain("Duration must be positive");
    }

    [Fact]
    public async Task ValidateAppointmentSlotAsync_UnavailableSlot_ShouldThrowException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var slotTime = GetNextMondayAt9AM();

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ValidateAppointmentSlotAsync(doctorId, slotTime, 60));

        exception.Message.Should().Contain("not available");
    }

    private static DateTime GetNextMondayAt9AM()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
            daysUntilMonday = 7; // Get next Monday if today is Monday
        return today.AddDays(daysUntilMonday).AddHours(9);
    }
}