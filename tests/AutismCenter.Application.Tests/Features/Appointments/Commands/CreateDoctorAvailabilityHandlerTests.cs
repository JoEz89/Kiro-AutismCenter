using AutismCenter.Application.Features.Appointments.Commands.CreateDoctorAvailability;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutismCenter.Application.Tests.Features.Appointments.Commands;

public class CreateDoctorAvailabilityHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly CreateDoctorAvailabilityHandler _handler;

    public CreateDoctorAvailabilityHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _handler = new CreateDoctorAvailabilityHandler(_doctorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateAvailability()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var command = new CreateDoctorAvailabilityCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _doctorRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DoctorId.Should().Be(doctorId);
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.StartTime.Should().Be(new TimeOnly(9, 0));
        result.EndTime.Should().Be(new TimeOnly(17, 0));
        result.IsActive.Should().BeTrue();

        _doctorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Doctor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoctorNotFound_ShouldThrowException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new CreateDoctorAvailabilityCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain($"Doctor with ID {doctorId} not found");
    }

    [Fact]
    public async Task Handle_InvalidTimeRange_ShouldThrowException()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var email = Email.Create("doctor@test.com");
        var doctor = Doctor.Create("Dr. John", "د. جون", "Autism Specialist", "أخصائي التوحد", email);
        
        var command = new CreateDoctorAvailabilityCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(17, 0), // End time before start time
            new TimeOnly(9, 0));

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Start time must be before end time");
    }
}