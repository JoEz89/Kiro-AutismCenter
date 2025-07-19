using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Domain.Entities;

public class Doctor : BaseEntity
{
    public string NameEn { get; private set; }
    public string NameAr { get; private set; }
    public string SpecialtyEn { get; private set; }
    public string SpecialtyAr { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public string? Biography { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<Appointment> _appointments = new();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private readonly List<DoctorAvailability> _availability = new();
    public IReadOnlyCollection<DoctorAvailability> Availability => _availability.AsReadOnly();

    private Doctor() { } // For EF Core

    private Doctor(string nameEn, string nameAr, string specialtyEn, string specialtyAr, Email email)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        SpecialtyEn = specialtyEn;
        SpecialtyAr = specialtyAr;
        Email = email;
        IsActive = true;
    }

    public static Doctor Create(string nameEn, string nameAr, string specialtyEn, string specialtyAr, Email email)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        if (string.IsNullOrWhiteSpace(specialtyEn))
            throw new ArgumentException("English specialty cannot be empty", nameof(specialtyEn));

        if (string.IsNullOrWhiteSpace(specialtyAr))
            throw new ArgumentException("Arabic specialty cannot be empty", nameof(specialtyAr));

        return new Doctor(nameEn.Trim(), nameAr.Trim(), specialtyEn.Trim(), specialtyAr.Trim(), email);
    }

    public void UpdateDetails(string nameEn, string nameAr, string specialtyEn, string specialtyAr,
                             PhoneNumber? phoneNumber = null, string? biography = null)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        if (string.IsNullOrWhiteSpace(specialtyEn))
            throw new ArgumentException("English specialty cannot be empty", nameof(specialtyEn));

        if (string.IsNullOrWhiteSpace(specialtyAr))
            throw new ArgumentException("Arabic specialty cannot be empty", nameof(specialtyAr));

        NameEn = nameEn.Trim();
        NameAr = nameAr.Trim();
        SpecialtyEn = specialtyEn.Trim();
        SpecialtyAr = specialtyAr.Trim();
        PhoneNumber = phoneNumber;
        Biography = biography?.Trim();
        UpdateTimestamp();
    }

    public void ChangeEmail(Email newEmail)
    {
        if (Email == newEmail)
            return;

        Email = newEmail;
        UpdateTimestamp();
    }

    public void AddAvailability(DoctorAvailability availability)
    {
        // Check for overlapping availability
        var overlapping = _availability.Any(a => a.OverlapsWith(availability));
        if (overlapping)
            throw new InvalidOperationException("Availability overlaps with existing schedule");

        _availability.Add(availability);
        UpdateTimestamp();
    }

    public void RemoveAvailability(Guid availabilityId)
    {
        var availability = _availability.FirstOrDefault(a => a.Id == availabilityId);
        if (availability != null)
        {
            _availability.Remove(availability);
            UpdateTimestamp();
        }
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

    public string GetName(bool isArabic) => isArabic ? NameAr : NameEn;

    public string GetSpecialty(bool isArabic) => isArabic ? SpecialtyAr : SpecialtyEn;

    public bool IsAvailableAt(DateTime dateTime, int durationInMinutes)
    {
        if (!IsActive)
            return false;

        var endTime = dateTime.AddMinutes(durationInMinutes);
        return _availability.Any(a => a.IsAvailableAt(dateTime, endTime));
    }

    public bool HasConflictingAppointment(DateTime dateTime, int durationInMinutes)
    {
        var endTime = dateTime.AddMinutes(durationInMinutes);
        return _appointments.Any(a => 
            a.Status != AppointmentStatus.Cancelled &&
            a.AppointmentDate < endTime &&
            a.GetEndTime() > dateTime);
    }
}