using MediatR;

namespace AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;

public record DownloadCertificateQuery(Guid EnrollmentId, Guid UserId) : IRequest<DownloadCertificateResponse>;