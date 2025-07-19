using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.ValueObjects;

public class PatientInfo : ValueObject
{
    public string PatientName { get; private set; }
    public int PatientAge { get; private set; }
    public string? MedicalHistory { get; private set; }
    public string? CurrentConcerns { get; private set; }
    public string? EmergencyContact { get; private set; }
    public PhoneNumber? EmergencyPhone { get; private set; }

    private PatientInfo(string patientName, int patientAge, string? medicalHistory = null,
                       string? currentConcerns = null, string? emergencyContact = null, PhoneNumber? emergencyPhone = null)
    {
        PatientName = patientName;
        PatientAge = patientAge;
        MedicalHistory = medicalHistory;
        CurrentConcerns = currentConcerns;
        EmergencyContact = emergencyContact;
        EmergencyPhone = emergencyPhone;
    }

    public static PatientInfo Create(string patientName, int patientAge, string? medicalHistory = null,
                                   string? currentConcerns = null, string? emergencyContact = null, PhoneNumber? emergencyPhone = null)
    {
        if (string.IsNullOrWhiteSpace(patientName))
            throw new ArgumentException("Patient name cannot be empty", nameof(patientName));

        if (patientAge < 0 || patientAge > 150)
            throw new ArgumentException("Patient age must be between 0 and 150", nameof(patientAge));

        return new PatientInfo(patientName.Trim(), patientAge, medicalHistory?.Trim(),
                              currentConcerns?.Trim(), emergencyContact?.Trim(), emergencyPhone);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PatientName;
        yield return PatientAge;
        yield return MedicalHistory ?? string.Empty;
        yield return CurrentConcerns ?? string.Empty;
        yield return EmergencyContact ?? string.Empty;
        yield return EmergencyPhone?.Value ?? string.Empty;
    }
}