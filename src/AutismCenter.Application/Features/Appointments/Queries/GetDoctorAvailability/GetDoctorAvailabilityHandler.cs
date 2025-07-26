using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetDoctorAvailability;

public class GetDoctorAvailabilityHandler : IRequestHandler<GetDoctorAvailabilityQuery, GetDoctorAvailabilityResponse>
{
    private readonly IDoctorRepository _doctorRepository;

    public GetDoctorAvailabilityHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<GetDoctorAvailabilityResponse> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found");

        var availabilityDtos = doctor.Availability
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .Select(a => new DoctorAvailabilityDto(
                a.Id,
                a.DayOfWeek,
                a.StartTime,
                a.EndTime,
                a.IsActive))
            .ToList();

        return new GetDoctorAvailabilityResponse(
            doctor.Id,
            doctor.NameEn,
            doctor.NameAr,
            availabilityDtos);
    }
}