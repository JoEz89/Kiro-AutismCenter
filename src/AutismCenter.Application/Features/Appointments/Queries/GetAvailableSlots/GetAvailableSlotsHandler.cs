using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetAvailableSlots;

public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsQuery, GetAvailableSlotsResponse>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAvailableSlotsHandler(IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetAvailableSlotsResponse> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.Date;
        var endDate = request.EndDate ?? startDate.AddDays(30); // Default to 30 days from start date
        
        var doctors = request.DoctorId.HasValue 
            ? new[] { await _doctorRepository.GetByIdAsync(request.DoctorId.Value, cancellationToken) }.Where(d => d != null)
            : await _doctorRepository.GetActiveAsync(cancellationToken);

        var doctorSlots = new List<DoctorSlotsDto>();

        foreach (var doctor in doctors)
        {
            if (doctor == null) continue;

            var slots = await GenerateSlotsForDoctor(doctor, startDate, endDate, request.DurationInMinutes, cancellationToken);
            
            doctorSlots.Add(new DoctorSlotsDto(
                doctor.Id,
                doctor.NameEn,
                doctor.NameAr,
                doctor.SpecialtyEn,
                doctor.SpecialtyAr,
                slots));
        }

        return new GetAvailableSlotsResponse(doctorSlots);
    }

    private async Task<IEnumerable<AppointmentSlotDto>> GenerateSlotsForDoctor(
        AutismCenter.Domain.Entities.Doctor doctor, 
        DateTime startDate, 
        DateTime endDate, 
        int durationInMinutes, 
        CancellationToken cancellationToken)
    {
        var slots = new List<AppointmentSlotDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            // Skip past dates
            if (currentDate.Date < DateTime.UtcNow.Date)
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var dayOfWeek = currentDate.DayOfWeek;
            var availability = doctor.Availability
                .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
                .ToList();

            foreach (var avail in availability)
            {
                var slotStart = currentDate.Date.Add(avail.StartTime.ToTimeSpan());
                var availabilityEnd = currentDate.Date.Add(avail.EndTime.ToTimeSpan());

                // Skip past time slots for today
                if (currentDate.Date == DateTime.UtcNow.Date && slotStart <= DateTime.UtcNow.AddMinutes(30))
                {
                    // Round up to next slot that's at least 30 minutes from now
                    var nextValidTime = DateTime.UtcNow.AddMinutes(30);
                    var minutesToRound = durationInMinutes - (nextValidTime.Minute % durationInMinutes);
                    if (minutesToRound == durationInMinutes) minutesToRound = 0;
                    slotStart = nextValidTime.AddMinutes(minutesToRound);
                    slotStart = new DateTime(slotStart.Year, slotStart.Month, slotStart.Day, slotStart.Hour, slotStart.Minute, 0);
                }

                while (slotStart.AddMinutes(durationInMinutes) <= availabilityEnd)
                {
                    var slotEnd = slotStart.AddMinutes(durationInMinutes);
                    
                    // Check if slot conflicts with existing appointments
                    var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                        doctor.Id, slotStart, slotEnd, null, cancellationToken);

                    slots.Add(new AppointmentSlotDto(
                        slotStart,
                        slotEnd,
                        durationInMinutes,
                        !hasConflict));

                    slotStart = slotStart.AddMinutes(durationInMinutes);
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return slots.OrderBy(s => s.StartTime);
    }
}