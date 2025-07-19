using AutismCenter.Domain.Common;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime AppointmentDate { get; private set; }
    public int DurationInMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? ZoomMeetingId { get; private set; }
    public string? ZoomJoinUrl { get; private set; }
    public string? Notes { get; private set; }
    public string AppointmentNumber { get; private set; }
    public PatientInfo PatientInfo { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Doctor Doctor { get; private set; } = null!;

    private Appointment() { } // For EF Core

    private Appointment(Guid userId, Guid doctorId, DateTime appointmentDate, int durationInMinutes,
                       string appointmentNumber, PatientInfo patientInfo)
    {
        UserId = userId;
        DoctorId = doctorId;
        AppointmentDate = appointmentDate;
        DurationInMinutes = durationInMinutes;
        Status = AppointmentStatus.Scheduled;
        AppointmentNumber = appointmentNumber;
        PatientInfo = patientInfo;
    }

    public static Appointment Create(Guid userId, Guid doctorId, DateTime appointmentDate, int durationInMinutes,
                                   string appointmentNumber, PatientInfo patientInfo)
    {
        if (appointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future", nameof(appointmentDate));

        if (durationInMinutes <= 0)
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));

        if (string.IsNullOrWhiteSpace(appointmentNumber))
            throw new ArgumentException("Appointment number cannot be empty", nameof(appointmentNumber));

        var appointment = new Appointment(userId, doctorId, appointmentDate, durationInMinutes,
                                        appointmentNumber, patientInfo);

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment.Id, userId, doctorId, appointmentDate));

        return appointment;
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot confirm appointment with status {Status}");

        Status = AppointmentStatus.Confirmed;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentConfirmedEvent(Id, UserId, DoctorId, AppointmentDate));
    }

    public void Start()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException($"Cannot start appointment with status {Status}");

        Status = AppointmentStatus.InProgress;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentStartedEvent(Id, UserId, DoctorId));
    }

    public void Complete(string? notes = null)
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete appointment with status {Status}");

        Status = AppointmentStatus.Completed;
        Notes = notes;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentCompletedEvent(Id, UserId, DoctorId, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel appointment with status {Status}");

        Status = AppointmentStatus.Cancelled;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentCancelledEvent(Id, UserId, DoctorId, AppointmentDate));
    }

    public void MarkAsNoShow()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot mark as no-show appointment with status {Status}");

        Status = AppointmentStatus.NoShow;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentNoShowEvent(Id, UserId, DoctorId, AppointmentDate));
    }

    public void Reschedule(DateTime newAppointmentDate)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot reschedule appointment with status {Status}");

        if (newAppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("New appointment date must be in the future", nameof(newAppointmentDate));

        var oldDate = AppointmentDate;
        AppointmentDate = newAppointmentDate;
        Status = AppointmentStatus.Scheduled; // Reset to scheduled
        UpdateTimestamp();

        AddDomainEvent(new AppointmentRescheduledEvent(Id, UserId, DoctorId, oldDate, newAppointmentDate));
    }

    public void SetZoomMeeting(string meetingId, string joinUrl)
    {
        if (string.IsNullOrWhiteSpace(meetingId))
            throw new ArgumentException("Meeting ID cannot be empty", nameof(meetingId));

        if (string.IsNullOrWhiteSpace(joinUrl))
            throw new ArgumentException("Join URL cannot be empty", nameof(joinUrl));

        ZoomMeetingId = meetingId;
        ZoomJoinUrl = joinUrl;
        UpdateTimestamp();

        AddDomainEvent(new AppointmentZoomMeetingCreatedEvent(Id, meetingId, joinUrl));
    }

    public void UpdatePatientInfo(PatientInfo newPatientInfo)
    {
        PatientInfo = newPatientInfo;
        UpdateTimestamp();
    }

    public void AddNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return;

        Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes}\n{notes}";
        UpdateTimestamp();
    }

    public bool CanBeCancelled() => Status != AppointmentStatus.Completed && Status != AppointmentStatus.Cancelled;

    public bool CanBeRescheduled() => Status != AppointmentStatus.Completed && Status != AppointmentStatus.Cancelled;

    public bool IsUpcoming() => AppointmentDate > DateTime.UtcNow && Status != AppointmentStatus.Cancelled;

    public bool IsToday() => AppointmentDate.Date == DateTime.UtcNow.Date;

    public DateTime GetEndTime() => AppointmentDate.AddMinutes(DurationInMinutes);

    public bool HasZoomMeeting() => !string.IsNullOrEmpty(ZoomMeetingId);
}