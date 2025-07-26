using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Queries.GetEnrollmentProgress;

public class GetEnrollmentProgressHandler : IRequestHandler<GetEnrollmentProgressQuery, GetEnrollmentProgressResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public GetEnrollmentProgressHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<GetEnrollmentProgressResponse> Handle(GetEnrollmentProgressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment with progress details
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new GetEnrollmentProgressResponse(
                    false,
                    "Enrollment not found",
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            var enrollmentDto = EnrollmentDto.FromEntity(enrollment);
            var moduleProgressDtos = enrollment.ModuleProgressList
                .Select(ModuleProgressDto.FromEntity)
                .ToList();

            return new GetEnrollmentProgressResponse(
                true,
                "Enrollment progress retrieved successfully",
                enrollmentDto,
                moduleProgressDtos
            );
        }
        catch (Exception ex)
        {
            return new GetEnrollmentProgressResponse(
                false,
                "An error occurred while retrieving enrollment progress",
                null,
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}