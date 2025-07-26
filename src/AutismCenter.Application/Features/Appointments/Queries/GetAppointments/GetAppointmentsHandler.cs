using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, GetAppointmentsResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetAppointmentsResponse> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<AutismCenter.Domain.Entities.Appointment> appointments;

        if (request.UpcomingOnly)
        {
            appointments = await _appointmentRepository.GetUpcomingAppointmentsAsync(cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            appointments = await _appointmentRepository.GetByDateRangeAsync(
                request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else if (request.UserId.HasValue)
        {
            appointments = await _appointmentRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.DoctorId.HasValue)
        {
            appointments = await _appointmentRepository.GetByDoctorIdAsync(request.DoctorId.Value, cancellationToken);
        }
        else
        {
            appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        }

        var appointmentDtos = appointments
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentDto(
                a.Id,
                a.AppointmentNumber,
                a.UserId,
                a.DoctorId,
                a.Doctor?.NameEn ?? "",
                a.Doctor?.NameAr ?? "",
                a.Doctor?.SpecialtyEn ?? "",
                a.Doctor?.SpecialtyAr ?? "",
                a.AppointmentDate,
                a.GetEndTime(),
                a.DurationInMinutes,
                a.Status,
                a.PatientInfo.PatientName,
                a.PatientInfo.PatientAge,
                a.PatientInfo.MedicalHistory,
                a.PatientInfo.CurrentConcerns,
                a.PatientInfo.EmergencyContact,
                a.PatientInfo.EmergencyPhone,
                a.ZoomMeetingId,
                a.ZoomJoinUrl,
                a.Notes,
                a.CreatedAt))
            .ToList();

        return new GetAppointmentsResponse(appointmentDtos);
    }
}