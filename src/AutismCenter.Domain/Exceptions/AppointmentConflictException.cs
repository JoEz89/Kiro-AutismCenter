namespace AutismCenter.Domain.Exceptions;

public class AppointmentConflictException : DomainException
{
    public Guid DoctorId { get; }
    public DateTime RequestedDateTime { get; }

    public AppointmentConflictException(Guid doctorId, DateTime requestedDateTime) 
        : base($"Doctor '{doctorId}' already has an appointment at '{requestedDateTime:yyyy-MM-dd HH:mm}'")
    {
        DoctorId = doctorId;
        RequestedDateTime = requestedDateTime;
    }
}