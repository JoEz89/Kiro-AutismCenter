using AutismCenter.Application.Features.Appointments.Commands.BookAppointment;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Appointments.Commands;

public class BookAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAppointmentSlotService> _appointmentSlotServiceMock;
    private readonly Mock<IAppointmentZoomService> _appointmentZoomServiceMock;
    private readonly BookAppointmentHandler _handler;

    public BookAppointmentHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _appointmentSlotServiceMock = new Mock<IAppointmentSlotService>();
        _appointmentZoomServiceMock = new Mock<IAppointmentZoomService>();

        _handler = new BookAppointmentHandler(
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _userRepositoryMock.Object,
            _appointmentSlotServiceMock.Object,
            _appointmentZoomServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldBookAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);

        var userEmail = Email.Create("user@test.com");
        var user = User.Create(userEmail, "John", "Doe", Domain.Enums.UserRole.Patient);

        var doctorEmail = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. Smith", "د. سميث", "Autism Specialist", "أخصائي التوحد", doctorEmail);

        var command = new BookAppointmentCommand(
            userId,
            doctorId,
            appointmentDate,
            60,
            "John Doe Jr",
            8,
            "No medical history",
            "Behavioral concerns",
            "Jane Doe",
            "123-456-7890");

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentSlotServiceMock.Setup(x => x.ValidateAppointmentSlotAsync(
            doctorId, appointmentDate, 60, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock.Setup(x => x.GenerateAppointmentNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("APT-2024-001234");

        _appointmentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentZoomServiceMock.Setup(x => x.CreateMeetingForAppointmentAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://zoom.us/j/123456789");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.DoctorId.Should().Be(doctorId);
        result.AppointmentNumber.Should().Be("APT-2024-001234");
        result.AppointmentDate.Should().Be(appointmentDate);
        result.DurationInMinutes.Should().Be(60);
        result.PatientName.Should().Be("John Doe Jr");
        result.ZoomJoinUrl.Should().Be("https://zoom.us/j/123456789");

        _appointmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);

        var command = new BookAppointmentCommand(
            userId,
            doctorId,
            appointmentDate,
            60,
            "John Doe Jr",
            8);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"User with ID {userId} not found");
    }

    [Fact]
    public async Task Handle_DoctorNotFound_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);

        var userEmail = Email.Create("user@test.com");
        var user = User.Create(userEmail, "John", "Doe", Domain.Enums.UserRole.Patient);

        var command = new BookAppointmentCommand(
            userId,
            doctorId,
            appointmentDate,
            60,
            "John Doe Jr",
            8);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Doctor with ID {doctorId} not found");
    }

    [Fact]
    public async Task Handle_SlotNotAvailable_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);

        var userEmail = Email.Create("user@test.com");
        var user = User.Create(userEmail, "John", "Doe", Domain.Enums.UserRole.Patient);

        var doctorEmail = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. Smith", "د. سميث", "Autism Specialist", "أخصائي التوحد", doctorEmail);

        var command = new BookAppointmentCommand(
            userId,
            doctorId,
            appointmentDate,
            60,
            "John Doe Jr",
            8);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentSlotServiceMock.Setup(x => x.ValidateAppointmentSlotAsync(
            doctorId, appointmentDate, 60, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The selected time slot is not available"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("not available");
    }

    [Fact]
    public async Task Handle_ZoomServiceFails_ShouldStillBookAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);

        var userEmail = Email.Create("user@test.com");
        var user = User.Create(userEmail, "John", "Doe", Domain.Enums.UserRole.Patient);

        var doctorEmail = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. Smith", "د. سميث", "Autism Specialist", "أخصائي التوحد", doctorEmail);

        var command = new BookAppointmentCommand(
            userId,
            doctorId,
            appointmentDate,
            60,
            "John Doe Jr",
            8);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _appointmentSlotServiceMock.Setup(x => x.ValidateAppointmentSlotAsync(
            doctorId, appointmentDate, 60, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock.Setup(x => x.GenerateAppointmentNumberAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("APT-2024-001234");

        _appointmentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentZoomServiceMock.Setup(x => x.CreateMeetingForAppointmentAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Zoom service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ZoomJoinUrl.Should().BeNull(); // Zoom meeting creation failed
        _appointmentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}