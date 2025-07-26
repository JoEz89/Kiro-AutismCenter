using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Domain.Services;
using AutismCenter.Domain.ValueObjects;
using MediatR;

namespace AutismCenter.Application.Features.Appointments.Commands.BookAppointment;

public class BookAppointmentHandler : IRequestHandler<BookAppointmentCommand, BookAppointmentResponse>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentSlotService _appointmentSlotService;
    private readonly IAppointmentZoomService _appointmentZoomService;

    public BookAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IUserRepository userRepository,
        IAppointmentSlotService appointmentSlotService,
        IAppointmentZoomService appointmentZoomService)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _userRepository = userRepository;
        _appointmentSlotService = appointmentSlotService;
        _appointmentZoomService = appointmentZoomService;
    }

    public async Task<BookAppointmentResponse> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Validate doctor exists
        var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
            throw new InvalidOperationException($"Doctor with ID {request.DoctorId} not found");

        // Validate appointment slot is available
        await _appointmentSlotService.ValidateAppointmentSlotAsync(
            request.DoctorId, request.AppointmentDate, request.DurationInMinutes, null, cancellationToken);

        // Create patient info
        var patientInfo = PatientInfo.Create(
            request.PatientName,
            request.PatientAge,
            request.MedicalHistory,
            request.CurrentConcerns,
            request.EmergencyContact,
            request.EmergencyPhone);

        // Generate appointment number
        var appointmentNumber = await _appointmentRepository.GenerateAppointmentNumberAsync(cancellationToken);

        // Create appointment
        var appointment = Appointment.Create(
            request.UserId,
            request.DoctorId,
            request.AppointmentDate,
            request.DurationInMinutes,
            appointmentNumber,
            patientInfo);

        // Save appointment
        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        // Create Zoom meeting
        string? zoomJoinUrl = null;
        try
        {
            zoomJoinUrl = await _appointmentZoomService.CreateMeetingForAppointmentAsync(appointment, cancellationToken);
        }
        catch (Exception)
        {
            // Log error but don't fail the appointment booking
            // The Zoom meeting can be created later
        }

        return new BookAppointmentResponse(
            appointment.Id,
            appointment.AppointmentNumber,
            appointment.UserId,
            appointment.DoctorId,
            doctor.NameEn,
            doctor.NameAr,
            appointment.AppointmentDate,
            appointment.DurationInMinutes,
            appointment.Status,
            appointment.PatientInfo.PatientName,
            zoomJoinUrl);
    }
}