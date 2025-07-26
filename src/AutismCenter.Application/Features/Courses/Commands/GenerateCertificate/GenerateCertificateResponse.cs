using AutismCenter.Application.Features.Courses.Common;

namespace AutismCenter.Application.Features.Courses.Commands.GenerateCertificate;

public record GenerateCertificateResponse(
    bool IsSuccess,
    string Message,
    string? CertificateUrl = null,
    EnrollmentDto? Enrollment = null,
    Dictionary<string, string[]>? ValidationErrors = null
);