using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.ExtendEnrollment;

public class ExtendEnrollmentHandler : IRequestHandler<ExtendEnrollmentCommand, ExtendEnrollmentResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ExtendEnrollmentHandler(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ExtendEnrollmentResponse> Handle(ExtendEnrollmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new ExtendEnrollmentResponse(
                    false,
                    "Enrollment not found",
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            // Extend enrollment
            enrollment.ExtendExpiry(request.AdditionalDays);

            // Save changes
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            var enrollmentDto = EnrollmentDto.FromEntity(enrollment);

            return new ExtendEnrollmentResponse(
                true,
                $"Enrollment extended by {request.AdditionalDays} days",
                enrollmentDto
            );
        }
        catch (ArgumentException ex)
        {
            return new ExtendEnrollmentResponse(
                false,
                "Enrollment extension failed",
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new ExtendEnrollmentResponse(
                false,
                "An unexpected error occurred while extending enrollment",
                null,
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { ex.Message } }
                }
            );
        }
    }
}