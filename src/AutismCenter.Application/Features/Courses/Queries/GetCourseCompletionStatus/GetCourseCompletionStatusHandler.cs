using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Courses.Queries.GetCourseCompletionStatus;

public class GetCourseCompletionStatusHandler : IRequestHandler<GetCourseCompletionStatusQuery, GetCourseCompletionStatusResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetCourseCompletionStatusHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetCourseCompletionStatusResponse> Handle(GetCourseCompletionStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment with course and module progress details
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new GetCourseCompletionStatusResponse(
                    false,
                    "Enrollment not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            // Build module completion data
            var moduleCompletions = new List<ModuleCompletionDto>();
            var totalModules = enrollment.Course?.Modules?.Count ?? 0;
            var completedModules = 0;

            if (enrollment.Course?.Modules != null)
            {
                foreach (var module in enrollment.Course.Modules.OrderBy(m => m.Order))
                {
                    var moduleProgress = enrollment.ModuleProgressList
                        .FirstOrDefault(mp => mp.ModuleId == module.Id);

                    var progressPercentage = moduleProgress?.ProgressPercentage ?? 0;
                    var isCompleted = progressPercentage == 100;
                    
                    if (isCompleted)
                        completedModules++;

                    moduleCompletions.Add(new ModuleCompletionDto(
                        module.Id,
                        module.TitleEn,
                        module.TitleAr,
                        module.Order,
                        progressPercentage,
                        isCompleted,
                        moduleProgress?.CompletedAt,
                        moduleProgress?.WatchTimeInSeconds ?? 0
                    ));
                }
            }

            var completionStatus = new CourseCompletionStatusDto(
                enrollment.Id,
                enrollment.UserId,
                enrollment.CourseId,
                enrollment.Course?.TitleEn ?? string.Empty,
                enrollment.Course?.TitleAr ?? string.Empty,
                enrollment.ProgressPercentage,
                enrollment.IsCompleted(),
                enrollment.CompletionDate,
                enrollment.CertificateUrl,
                enrollment.HasCertificate(),
                totalModules,
                completedModules,
                moduleCompletions
            );

            return new GetCourseCompletionStatusResponse(
                true,
                "Course completion status retrieved successfully",
                completionStatus
            );
        }
        catch (Exception ex)
        {
            return new GetCourseCompletionStatusResponse(
                false,
                "An error occurred while retrieving course completion status",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}