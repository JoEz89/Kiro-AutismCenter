using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.UpdateProgress;

public class UpdateProgressHandler : IRequestHandler<UpdateProgressCommand, UpdateProgressResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public UpdateProgressHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<UpdateProgressResponse> Handle(UpdateProgressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new UpdateProgressResponse(
                    false,
                    "Enrollment not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            // Update progress
            enrollment.UpdateProgress(request.ModuleId, request.ProgressPercentage);

            // Update watch time if provided
            if (request.WatchTimeInSeconds > 0)
            {
                var moduleProgress = enrollment.ModuleProgressList
                    .FirstOrDefault(mp => mp.ModuleId == request.ModuleId);
                
                moduleProgress?.UpdateWatchTime(request.WatchTimeInSeconds);
            }

            // Save changes
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            var enrollmentDto = EnrollmentDto.FromEntity(enrollment);

            return new UpdateProgressResponse(
                true,
                "Progress updated successfully",
                enrollmentDto
            );
        }
        catch (InvalidOperationException ex)
        {
            return new UpdateProgressResponse(
                false,
                "Progress update failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (ArgumentException ex)
        {
            return new UpdateProgressResponse(
                false,
                "Progress update failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new UpdateProgressResponse(
                false,
                "An unexpected error occurred while updating progress",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}