using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AutismCenter.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly ILogger<CertificateService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _certificateStoragePath;

    public CertificateService(
        ILogger<CertificateService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _certificateStoragePath = _configuration["CertificateStorage:Path"] ?? "certificates";
        
        // Ensure certificate directory exists
        if (!Directory.Exists(_certificateStoragePath))
        {
            Directory.CreateDirectory(_certificateStoragePath);
        }
    }

    public async Task<string> GenerateCertificateAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating certificate for enrollment {EnrollmentId}", enrollment.Id);

            // Generate PDF certificate
            var certificateData = await GenerateCertificatePdfAsync(enrollment, cancellationToken);

            // Create filename
            var fileName = $"certificate_{enrollment.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

            // Store certificate
            var certificateUrl = await StoreCertificateAsync(certificateData, fileName, cancellationToken);

            _logger.LogInformation("Certificate generated successfully for enrollment {EnrollmentId}: {CertificateUrl}", 
                enrollment.Id, certificateUrl);

            return certificateUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate for enrollment {EnrollmentId}", enrollment.Id);
            throw;
        }
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, we'll create a simple text-based certificate
            // In a real implementation, you would use a PDF library like iTextSharp or PdfSharp
            var certificateContent = GenerateCertificateContent(enrollment);
            
            // Convert to bytes (in real implementation, this would be PDF bytes)
            var certificateBytes = Encoding.UTF8.GetBytes(certificateContent);
            
            _logger.LogInformation("PDF certificate generated for enrollment {EnrollmentId}", enrollment.Id);
            
            return await Task.FromResult(certificateBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF certificate for enrollment {EnrollmentId}", enrollment.Id);
            throw;
        }
    }

    public async Task<byte[]?> GetCertificateDataAsync(string certificateUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(certificateUrl))
                return null;

            // Extract filename from URL
            var fileName = Path.GetFileName(certificateUrl);
            var filePath = Path.Combine(_certificateStoragePath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Certificate file not found: {FilePath}", filePath);
                return null;
            }

            var certificateData = await File.ReadAllBytesAsync(filePath, cancellationToken);
            _logger.LogInformation("Certificate data retrieved: {FilePath}", filePath);
            
            return certificateData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving certificate data for URL {CertificateUrl}", certificateUrl);
            return null;
        }
    }

    public async Task<string> StoreCertificateAsync(byte[] certificateData, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_certificateStoragePath, fileName);
            
            await File.WriteAllBytesAsync(filePath, certificateData, cancellationToken);
            
            // Return relative URL (in real implementation, this might be a cloud storage URL)
            var certificateUrl = $"/certificates/{fileName}";
            
            _logger.LogInformation("Certificate stored at {FilePath}", filePath);
            
            return certificateUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing certificate {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteCertificateAsync(string certificateUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(certificateUrl))
                return false;

            // Extract filename from URL
            var fileName = Path.GetFileName(certificateUrl);
            var filePath = Path.Combine(_certificateStoragePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Certificate deleted: {FilePath}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting certificate {CertificateUrl}", certificateUrl);
            return false;
        }
    }

    private string GenerateCertificateContent(Enrollment enrollment)
    {
        var userName = $"{enrollment.User?.FirstName} {enrollment.User?.LastName}".Trim();
        var courseTitle = enrollment.Course?.TitleEn ?? "Course";
        var completionDate = enrollment.CompletionDate?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy");
        
        return $@"
CERTIFICATE OF COMPLETION

This is to certify that

{userName}

has successfully completed the course

{courseTitle}

on {completionDate}

Autism Center
Certificate ID: {enrollment.Id}
";
    }
}