namespace AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;

public record GetCourseCompletionStatusResponse(
    bool IsSuccess,
    string Message,
    CourseCompletionStatusDto? CompletionStatus = null,
    Dictionary<string, string[]>? ValidationErrors = null
);

public record CourseCompletionStatusDto(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    string CourseTitle,
    string CourseTitleAr,
    int OverallProgressPercentage,
    bool IsCompleted,
    DateTime? CompletionDate,
    string? CertificateUrl,
    bool HasCertificate,
    int TotalModules,
    int CompletedModules,
    List<ModuleCompletionDto> ModuleCompletions
);

public record ModuleCompletionDto(
    Guid ModuleId,
    string ModuleTitle,
    string ModuleTitleAr,
    int Order,
    int ProgressPercentage,
    bool IsCompleted,
    DateTime? CompletedAt,
    int WatchTimeInSeconds
);