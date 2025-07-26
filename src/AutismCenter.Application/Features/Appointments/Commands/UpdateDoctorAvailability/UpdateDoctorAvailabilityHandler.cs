using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.UpdateDoctorAvailability;

public class UpdateDoctorAvailabilityHandler : IRequestHandler<UpdateDoctorAvailabilityCommand, UpdateDoctorAvailabilityResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public UpdateDoctorAvailabilityHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<UpdateDoctorAvailabilityResponse> Handle(UpdateDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found");

        var availability = doctor.Availability.FirstOrDefault(a => a.Id == request.AvailabilityId);
        if (availability == null)
            throw new InvalidOperationException($"Availability with ID {request.AvailabilityId} not found");

        availability.UpdateTimes(request.StartTime, request.EndTime);
        
        if (request.IsActive && !availability.IsActive)
            availability.Activate();
        else if (!request.IsActive && availability.IsActive)
            availability.Deactivate();

        await _doctorRepository.UpdateAsync(doctor, cancellationToken);

        return new UpdateDoctorAvailabilityResponse(
            availability.Id,
            availability.DoctorId,
            availability.DayOfWeek,
            availability.StartTime,
            availability.EndTime,
            availability.IsActive);
    }
}