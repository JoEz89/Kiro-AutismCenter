using AutismCenter.Domain.Entities;

namespace AutismCenter.Domain.Interfaces;

public interface ICertificateService
{
    Task<string> GenerateCertificateAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCertificatePdfAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<byte[]?> GetCertificateDataAsync(string certificateUrl, CancellationToken cancellationToken = default);
    Task<string> StoreCertificateAsync(byte[] certificateData, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteCertificateAsync(string certificateUrl, CancellationToken cancellationToken = default);
}