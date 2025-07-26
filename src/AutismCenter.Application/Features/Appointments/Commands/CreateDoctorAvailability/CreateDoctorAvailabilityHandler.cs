using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.CreateDoctorAvailability;

public class CreateDoctorAvailabilityHandler : IRequestHandler<CreateDoctorAvailabilityCommand, CreateDoctorAvailabilityResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public CreateDoctorAvailabilityHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<CreateDoctorAvailabilityResponse> Handle(CreateDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found");

        var availability = DoctorAvailability.Create(
            request.DoctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime);

        doctor.AddAvailability(availability);
        await _doctorRepository.UpdateAsync(doctor, cancellationToken);

        return new CreateDoctorAvailabilityResponse(
            availability.Id,
            availability.DoctorId,
            availability.DayOfWeek,
            availability.StartTime,
            availability.EndTime,
            availability.IsActive);
    }
}