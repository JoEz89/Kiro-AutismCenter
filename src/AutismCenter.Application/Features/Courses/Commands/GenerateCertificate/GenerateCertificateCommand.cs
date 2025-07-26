using MediatR;

namespace AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;

public record GenerateCertificateCommand(Guid EnrollmentId) : IRequest<GenerateCertificateResponse>;