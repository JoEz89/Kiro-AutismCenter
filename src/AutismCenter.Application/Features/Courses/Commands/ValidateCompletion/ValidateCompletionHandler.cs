using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.ValidateCompletion;

public class ValidateCompletionHandler : IRequestHandler<ValidateCompletionCommand, ValidateCompletionResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ValidateCompletionHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ValidateCompletionResponse> Handle(ValidateCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new ValidateCompletionResponse(
                    false,
                    "Enrollment not found",
                    false,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            // Check if enrollment is active and not expired
            if (!enrollment.CanAccess())
            {
                return new ValidateCompletionResponse(
                    false,
                    "Cannot validate completion for inactive or expired enrollment",
                    false,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "Access", new[] { "Enrollment is inactive or expired" } }
                    }
                );
            }

            // Validate completion based on module progress
            var isCompleted = ValidateCourseCompletion(enrollment);
            
            // If course is completed but not marked as such, mark it as completed
            if (isCompleted && !enrollment.IsCompleted())
            {
                enrollment.MarkAsCompleted();
                await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
            }

            var enrollmentDto = EnrollmentDto.FromEntity(enrollment);

            return new ValidateCompletionResponse(
                true,
                isCompleted ? "Course is completed" : "Course is not yet completed",
                isCompleted,
                enrollmentDto
            );
        }
        catch (Exception ex)
        {
            return new ValidateCompletionResponse(
                false,
                "An error occurred while validating course completion",
                false,
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }

    private bool ValidateCourseCompletion(Domain.Entities.Enrollment enrollment)
    {
        // If no course modules, consider it completed if progress is 100%
        if (enrollment.Course?.Modules == null || !enrollment.Course.Modules.Any())
        {
            return enrollment.ProgressPercentage == 100;
        }

        // Check if all modules are completed
        var totalModules = enrollment.Course.Modules.Count;
        var completedModules = 0;

        foreach (var module in enrollment.Course.Modules)
        {
            var moduleProgress = enrollment.ModuleProgressList
                .FirstOrDefault(mp => mp.ModuleId == module.Id);

            if (moduleProgress != null && moduleProgress.IsCompleted())
            {
                completedModules++;
            }
        }

        // Course is completed if all modules are completed
        return completedModules == totalModules && totalModules > 0;
    }
}