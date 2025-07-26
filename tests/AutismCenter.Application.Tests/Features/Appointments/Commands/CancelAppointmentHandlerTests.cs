using AutismCenter.Application.Features.Appointments.Commands.CancelAppointment;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Appointments.Commands;

public class CancelAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IAppointmentZoomService> _appointmentZoomServiceMock;
    private readonly CancelAppointmentHandler _handler;

    public CancelAppointmentHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _appointmentZoomServiceMock = new Mock<IAppointmentZoomService>();
        _handler = new CancelAppointmentHandler(_appointmentRepositoryMock.Object, _appointmentZoomServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCancelAppointment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe Jr", 8);

        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        var command = new CancelAppointmentCommand(appointmentId, userId);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _appointmentZoomServiceMock.Setup(x => x.HandleAppointmentCancelledAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        appointment.Status.Should().Be(Domain.Enums.AppointmentStatus.Cancelled);
        _appointmentZoomServiceMock.Verify(x => x.HandleAppointmentCancelledAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ShouldThrowException()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CancelAppointmentCommand(appointmentId, userId);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Appointment with ID {appointmentId} not found");
    }

    [Fact]
    public async Task Handle_UserNotOwner_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe Jr", 8);

        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);

        var command = new CancelAppointmentCommand(appointmentId, otherUserId);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("You can only cancel your own appointments");
    }

    [Fact]
    public async Task Handle_AppointmentAlreadyCompleted_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentDate = DateTime.UtcNow.AddHours(2);
        var patientInfo = PatientInfo.Create("John Doe Jr", 8);

        var appointment = Appointment.Create(userId, doctorId, appointmentDate, 60, "APT-001", patientInfo);
        appointment.Start();
        appointment.Complete();

        var command = new CancelAppointmentCommand(appointmentId, userId);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Cannot cancel appointment with status");
    }
}