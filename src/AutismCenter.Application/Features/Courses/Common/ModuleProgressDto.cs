using AutismCenter.Domain.Entities;

namespace AutismCenter.Application.Features.Courses.Common;

public record ModuleProgressDto(
    Guid Id,
    Guid EnrollmentId,
    Guid ModuleId,
    string ModuleTitleEn,
    string ModuleTitleAr,
    int ProgressPercentage,
    DateTime? CompletedAt,
    int WatchTimeInSeconds,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static ModuleProgressDto FromEntity(ModuleProgress moduleProgress)
    {
        if (moduleProgress == null)
            throw new ArgumentNullException(nameof(moduleProgress));

        return new ModuleProgressDto(
            moduleProgress.Id,
            moduleProgress.EnrollmentId,
            moduleProgress.ModuleId,
            moduleProgress.Module?.TitleEn ?? string.Empty,
            moduleProgress.Module?.TitleAr ?? string.Empty,
            moduleProgress.ProgressPercentage,
            moduleProgress.CompletedAt,
            moduleProgress.WatchTimeInSeconds,
            moduleProgress.CreatedAt,
            moduleProgress.UpdatedAt
        );
    }

    public string GetModuleTitle(bool isArabic) => isArabic ? ModuleTitleAr : ModuleTitleEn;
    public TimeSpan GetWatchTime() => TimeSpan.FromSeconds(WatchTimeInSeconds);
    public bool IsCompleted() => ProgressPercentage == 100;
}