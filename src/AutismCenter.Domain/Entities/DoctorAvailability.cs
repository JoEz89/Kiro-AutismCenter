using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class DoctorAvailability : BaseEntity
{
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public Doctor Doctor { get; private set; } = null!;

    private DoctorAvailability() { } // For EF Core

    private DoctorAvailability(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        DoctorId = doctorId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = true;
    }

    public static DoctorAvailability Create(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        return new DoctorAvailability(doctorId, dayOfWeek, startTime, endTime);
    }

    public void UpdateTimes(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        StartTime = startTime;
        EndTime = endTime;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();
    }

    public bool IsAvailableAt(DateTime startDateTime, DateTime endDateTime)
    {
        if (!IsActive)
            return false;

        if (startDateTime.DayOfWeek != DayOfWeek)
            return false;

        var startTimeOnly = TimeOnly.FromDateTime(startDateTime);
        var endTimeOnly = TimeOnly.FromDateTime(endDateTime);

        return startTimeOnly >= StartTime && endTimeOnly <= EndTime;
    }

    public bool OverlapsWith(DoctorAvailability other)
    {
        if (DayOfWeek != other.DayOfWeek)
            return false;

        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public TimeSpan GetDuration() => EndTime - StartTime;
}