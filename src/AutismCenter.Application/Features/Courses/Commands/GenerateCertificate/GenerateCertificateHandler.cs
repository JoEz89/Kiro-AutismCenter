using MediatR;
using AutismCenter.Domain.Interfaces;
using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;

public class GenerateCertificateHandler : IRequestHandler<GenerateCertificateCommand, GenerateCertificateResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICertificateService _certificateService;

    public GenerateCertificateHandler(
        IEnrollmentRepository enrollmentRepository,
        ICertificateService certificateService)
    {
        _enrollmentRepository = enrollmentRepository;
        _certificateService = certificateService;
    }

    public async Task<GenerateCertificateResponse> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new GenerateCertificateResponse(
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

            // Validate enrollment is completed
            if (!enrollment.IsCompleted())
            {
                return new GenerateCertificateResponse(
                    false,
                    "Certificate can only be generated for completed courses",
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "Completion", new[] { "Course must be completed before generating certificate" } }
                    }
                );
            }

            // Check if certificate already exists
            if (enrollment.HasCertificate())
            {
                var enrollmentDto = EnrollmentDto.FromEntity(enrollment);
                return new GenerateCertificateResponse(
                    true,
                    "Certificate already exists",
                    enrollment.CertificateUrl,
                    enrollmentDto
                );
            }

            // Generate certificate
            var certificateUrl = await _certificateService.GenerateCertificateAsync(enrollment, cancellationToken);

            // Update enrollment with certificate URL
            enrollment.GenerateCertificate(certificateUrl);

            // Save changes
            await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

            var updatedEnrollmentDto = EnrollmentDto.FromEntity(enrollment);

            return new GenerateCertificateResponse(
                true,
                "Certificate generated successfully",
                certificateUrl,
                updatedEnrollmentDto
            );
        }
        catch (InvalidOperationException ex)
        {
            return new GenerateCertificateResponse(
                false,
                "Certificate generation failed",
                null,
                null,
                new Dictionary<string, string[]>
                {
                    { "ValidationError", new[] { ex.Message } }
                }
            );
        }
        catch (Exception ex)
        {
            return new GenerateCertificateResponse(
                false,
                "An unexpected error occurred while generating certificate",
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