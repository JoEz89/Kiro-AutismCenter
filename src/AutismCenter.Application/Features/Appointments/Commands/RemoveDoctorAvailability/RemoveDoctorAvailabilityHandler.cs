using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.RemoveDoctorAvailability;

public class RemoveDoctorAvailabilityHandler : IRequestHandler<RemoveDoctorAvailabilityCommand>
{
    private readonly IDoctorRepository _doctorRepository;

    public RemoveDoctorAvailabilityHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task Handle(RemoveDoctorAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found");

        doctor.RemoveAvailability(request.AvailabilityId);
        await _doctorRepository.UpdateAsync(doctor, cancellationToken);
    }
}