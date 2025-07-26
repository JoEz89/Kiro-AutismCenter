namespace AutismCenter.Application.Features.Courses.Queries.DownloadCertificate;

public record DownloadCertificateResponse(
    bool IsSuccess,
    string Message,
    byte[]? CertificateData = null,
    string? FileName = null,
    string? ContentType = null,
    Dictionary<string, string[]>? ValidationErrors = null
);