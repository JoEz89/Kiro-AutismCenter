using AutismCenter.Domain.Common;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid CourseId { get; private set; }
    public DateTime EnrollmentDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public int ProgressPercentage { get; private set; }
    public DateTime? CompletionDate { get; private set; }
    public string? CertificateUrl { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Course Course { get; private set; } = null!;

    private readonly List<ModuleProgress> _moduleProgress = new();
    public IReadOnlyCollection<ModuleProgress> ModuleProgressList => _moduleProgress.AsReadOnly();

    private Enrollment() { } // For EF Core

    private Enrollment(Guid userId, Guid courseId, DateTime enrollmentDate, DateTime expiryDate)
    {
        UserId = userId;
        CourseId = courseId;
        EnrollmentDate = enrollmentDate;
        ExpiryDate = expiryDate;
        ProgressPercentage = 0;
        IsActive = true;
    }

    public static Enrollment CreateEnrollment(Guid userId, Guid courseId, int validityDays = 30)
    {
        var enrollmentDate = DateTime.UtcNow;
        var expiryDate = enrollmentDate.AddDays(validityDays);

        var enrollment = new Enrollment(userId, courseId, enrollmentDate, expiryDate);
        
        enrollment.AddDomainEvent(new EnrollmentCreatedEvent(enrollment.Id, userId, courseId, enrollmentDate, expiryDate));
        
        return enrollment;
    }

    public void UpdateProgress(Guid moduleId, int progressPercentage)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update progress for inactive enrollment");

        if (IsExpired())
            throw new InvalidOperationException("Cannot update progress for expired enrollment");

        if (progressPercentage < 0 || progressPercentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(progressPercentage));

        var moduleProgress = _moduleProgress.FirstOrDefault(mp => mp.ModuleId == moduleId);
        if (moduleProgress == null)
        {
            moduleProgress = ModuleProgress.CreateProgress(Id, moduleId, progressPercentage);
            _moduleProgress.Add(moduleProgress);
        }
        else
        {
            moduleProgress.UpdateProgress(progressPercentage);
        }

        RecalculateOverallProgress();
        UpdateTimestamp();

        if (ProgressPercentage == 100 && CompletionDate == null)
        {
            MarkAsCompleted();
        }
    }

    public void MarkAsCompleted()
    {
        if (CompletionDate != null)
            return;

        CompletionDate = DateTime.UtcNow;
        ProgressPercentage = 100;
        UpdateTimestamp();

        AddDomainEvent(new EnrollmentCompletedEvent(Id, UserId, CourseId, CompletionDate.Value));
    }

    public void GenerateCertificate(string certificateUrl)
    {
        if (CompletionDate == null)
            throw new InvalidOperationException("Cannot generate certificate for incomplete course");

        if (string.IsNullOrWhiteSpace(certificateUrl))
            throw new ArgumentException("Certificate URL cannot be empty", nameof(certificateUrl));

        CertificateUrl = certificateUrl;
        UpdateTimestamp();

        AddDomainEvent(new CertificateGeneratedEvent(Id, UserId, CourseId, certificateUrl));
    }

    public void ExtendExpiry(int additionalDays)
    {
        if (additionalDays <= 0)
            throw new ArgumentException("Additional days must be positive", nameof(additionalDays));

        ExpiryDate = ExpiryDate.AddDays(additionalDays);
        UpdateTimestamp();

        AddDomainEvent(new EnrollmentExtendedEvent(Id, UserId, CourseId, ExpiryDate));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();

        AddDomainEvent(new EnrollmentDeactivatedEvent(Id, UserId, CourseId));
    }

    private void RecalculateOverallProgress()
    {
        if (!_moduleProgress.Any())
        {
            ProgressPercentage = 0;
            return;
        }

        var averageProgress = _moduleProgress.Average(mp => mp.ProgressPercentage);
        ProgressPercentage = (int)Math.Round(averageProgress);
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiryDate;

    public bool IsCompleted() => CompletionDate != null;

    public bool HasCertificate() => !string.IsNullOrEmpty(CertificateUrl);

    public bool CanAccess() => IsActive && !IsExpired();

    public int GetDaysRemaining()
    {
        if (IsExpired())
            return 0;

        return (int)(ExpiryDate - DateTime.UtcNow).TotalDays;
    }
}