using AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Appointments.Queries;

public class GetAvailableSlotsHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAvailableSlotsHandler _handler;

    public GetAvailableSlotsHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _handler = new GetAvailableSlotsHandler(_doctorRepositoryMock.Object, _appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldReturnAvailableSlots()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        // Add availability for Monday 9 AM to 5 PM
        var availability = DoctorAvailability.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0));
        doctor.AddAvailability(availability);

        var startDate = GetNextMonday();
        var query = new GetAvailableSlotsQuery(doctorId, startDate, startDate.AddDays(1), 60);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentRepositoryMock.Setup(x => x.HasConflictingAppointmentAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DoctorSlots.Should().HaveCount(1);
        
        var doctorSlots = result.DoctorSlots.First();
        doctorSlots.DoctorId.Should().Be(doctorId);
        doctorSlots.DoctorNameEn.Should().Be("Dr. John");
        doctorSlots.AvailableSlots.Should().NotBeEmpty();
        doctorSlots.AvailableSlots.Should().OnlyContain(s => s.IsAvailable);
    }

    [Fact]
    public async Task Handle_NoAvailability_ShouldReturnEmptySlots()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var startDate = GetNextMonday();
        var query = new GetAvailableSlotsQuery(doctorId, startDate, startDate.AddDays(1), 60);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DoctorSlots.Should().HaveCount(1);
        
        var doctorSlots = result.DoctorSlots.First();
        doctorSlots.AvailableSlots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConflictingAppointments_ShouldMarkSlotsAsUnavailable()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var availability = DoctorAvailability.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0));
        doctor.AddAvailability(availability);

        var startDate = GetNextMonday();
        var query = new GetAvailableSlotsQuery(doctorId, startDate, startDate.AddDays(1), 60);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // First slot has conflict, second doesn't
        _appointmentRepositoryMock.SetupSequence(x => x.HasConflictingAppointmentAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)  // 9-10 AM has conflict
            .ReturnsAsync(false); // 10-11 AM is available

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var doctorSlots = result.DoctorSlots.First();
        doctorSlots.AvailableSlots.Should().HaveCount(2);
        
        var slots = doctorSlots.AvailableSlots.ToList();
        slots[0].IsAvailable.Should().BeFalse(); // 9-10 AM
        slots[1].IsAvailable.Should().BeTrue();  // 10-11 AM
    }

    private static DateTime GetNextMonday()
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && DateTime.UtcNow.TimeOfDay > TimeSpan.FromHours(12))
            daysUntilMonday = 7; // If it's Monday afternoon, get next Monday
        return today.AddDays(daysUntilMonday);
    }
}