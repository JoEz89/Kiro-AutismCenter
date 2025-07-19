using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class ModuleProgress : BaseEntity
{
    public Guid EnrollmentId { get; private set; }
    public Guid ModuleId { get; private set; }
    public int ProgressPercentage { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int WatchTimeInSeconds { get; private set; }

    // Navigation properties
    public Enrollment Enrollment { get; private set; } = null!;
    public CourseModule Module { get; private set; } = null!;

    private ModuleProgress() { } // For EF Core

    private ModuleProgress(Guid enrollmentId, Guid moduleId, int progressPercentage)
    {
        EnrollmentId = enrollmentId;
        ModuleId = moduleId;
        ProgressPercentage = progressPercentage;
        WatchTimeInSeconds = 0;
    }

    public static ModuleProgress CreateProgress(Guid enrollmentId, Guid moduleId, int progressPercentage)
    {
        if (progressPercentage < 0 || progressPercentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(progressPercentage));

        return new ModuleProgress(enrollmentId, moduleId, progressPercentage);
    }

    public void UpdateProgress(int newProgressPercentage)
    {
        if (newProgressPercentage < 0 || newProgressPercentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(newProgressPercentage));

        ProgressPercentage = newProgressPercentage;
        UpdateTimestamp();

        if (newProgressPercentage == 100 && CompletedAt == null)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (newProgressPercentage < 100 && CompletedAt != null)
        {
            CompletedAt = null; // Reset completion if progress goes back down
        }
    }

    public void UpdateWatchTime(int watchTimeInSeconds)
    {
        if (watchTimeInSeconds < 0)
            throw new ArgumentException("Watch time cannot be negative", nameof(watchTimeInSeconds));

        WatchTimeInSeconds = watchTimeInSeconds;
        UpdateTimestamp();
    }

    public bool IsCompleted() => ProgressPercentage == 100;

    public TimeSpan GetWatchTime() => TimeSpan.FromSeconds(WatchTimeInSeconds);
}