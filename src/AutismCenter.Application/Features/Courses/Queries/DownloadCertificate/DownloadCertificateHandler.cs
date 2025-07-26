using MediatR;
using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;

public class DownloadCertificateHandler : IRequestHandler<DownloadCertificateQuery, DownloadCertificateResponse>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICertificateService _certificateService;

    public DownloadCertificateHandler(
        IEnrollmentRepository enrollmentRepository,
        ICertificateService certificateService)
    {
        _enrollmentRepository = enrollmentRepository;
        _certificateService = certificateService;
    }

    public async Task<DownloadCertificateResponse> Handle(DownloadCertificateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get enrollment
            var enrollment = await _enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken);
            if (enrollment == null)
            {
                return new DownloadCertificateResponse(
                    false,
                    "Enrollment not found",
                    null,
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { nameof(request.EnrollmentId), new[] { "Enrollment with the specified ID does not exist" } }
                    }
                );
            }

            // Verify user owns this enrollment
            if (enrollment.UserId != request.UserId)
            {
                return new DownloadCertificateResponse(
                    false,
                    "Access denied",
                    null,
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "Authorization", new[] { "You are not authorized to download this certificate" } }
                    }
                );
            }

            // Check if certificate exists
            if (!enrollment.HasCertificate())
            {
                return new DownloadCertificateResponse(
                    false,
                    "Certificate not available",
                    null,
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "Certificate", new[] { "Certificate has not been generated for this enrollment" } }
                    }
                );
            }

            // Get certificate data from storage
            var certificateData = await _certificateService.GetCertificateDataAsync(enrollment.CertificateUrl!, cancellationToken);
            
            if (certificateData == null)
            {
                return new DownloadCertificateResponse(
                    false,
                    "Certificate file not found",
                    null,
                    null,
                    null,
                    new Dictionary<string, string[]>
                    {
                        { "File", new[] { "Certificate file could not be found on the server" } }
                    }
                );
            }

            var downloadFileName = $"certificate_{enrollment.Course?.TitleEn?.Replace(" ", "_") ?? "course"}_{enrollment.User?.FirstName}_{enrollment.User?.LastName}.pdf";

            return new DownloadCertificateResponse(
                true,
                "Certificate downloaded successfully",
                certificateData,
                downloadFileName,
                "application/pdf"
            );
        }
        catch (Exception ex)
        {
            return new DownloadCertificateResponse(
                false,
                "An error occurred while downloading the certificate",
                null,
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