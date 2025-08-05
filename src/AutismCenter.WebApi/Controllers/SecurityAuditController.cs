using AutismCenter.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutismCenter.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SecurityAuditController : ControllerBase
{
    private readonly IDataBackupService _backupService;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly IPciComplianceService _pciComplianceService;
    private readonly ILogger<SecurityAuditController> _logger;

    public SecurityAuditController(
        IDataBackupService backupService,
        IAuditLoggingService auditLoggingService,
        IPciComplianceService pciComplianceService,
        ILogger<SecurityAuditController> logger)
    {
        _backupService = backupService;
        _auditLoggingService = auditLoggingService;
        _pciComplianceService = pciComplianceService;
        _logger = logger;
    }

    /// <summary>
    /// Get available backups
    /// </summary>
    [HttpGet("backups")]
    public async Task<ActionResult<List<BackupInfo>>> GetAvailableBackups()
    {
        try
        {
            var backups = await _backupService.GetAvailableBackupsAsync();
            
            await _auditLoggingService.LogDataAccessAsync(
                "BACKUP_INFO",
                "VIEW",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve backup information");
            return StatusCode(500, new { message = "Failed to retrieve backup information" });
        }
    }

    /// <summary>
    /// Create a manual database backup
    /// </summary>
    [HttpPost("backups/database")]
    public async Task<ActionResult<BackupCreationResponse>> CreateDatabaseBackup([FromBody] CreateBackupRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            await _auditLoggingService.LogUserActionAsync(
                "MANUAL_DATABASE_BACKUP_INITIATED",
                userId,
                $"Backup name: {request.BackupName}");

            var backupPath = await _backupService.CreateDatabaseBackupAsync(request.BackupName);
            var backupInfo = new FileInfo(backupPath);

            var response = new BackupCreationResponse
            {
                BackupPath = backupPath,
                FileName = Path.GetFileName(backupPath),
                Size = backupInfo.Length,
                CreatedAt = backupInfo.CreationTimeUtc,
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            
            await _auditLoggingService.LogUserActionAsync(
                "MANUAL_DATABASE_BACKUP_FAILED",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                ex.Message);

            return StatusCode(500, new { message = "Failed to create database backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a manual application data backup
    /// </summary>
    [HttpPost("backups/application")]
    public async Task<ActionResult<BackupCreationResponse>> CreateApplicationBackup()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            await _auditLoggingService.LogUserActionAsync(
                "MANUAL_APPLICATION_BACKUP_INITIATED",
                userId);

            var backupPath = await _backupService.CreateApplicationDataBackupAsync();
            var backupInfo = new FileInfo(backupPath);

            var response = new BackupCreationResponse
            {
                BackupPath = backupPath,
                FileName = Path.GetFileName(backupPath),
                Size = backupInfo.Length,
                CreatedAt = backupInfo.CreationTimeUtc,
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create application backup");
            
            await _auditLoggingService.LogUserActionAsync(
                "MANUAL_APPLICATION_BACKUP_FAILED",
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                ex.Message);

            return StatusCode(500, new { message = "Failed to create application backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Verify backup integrity
    /// </summary>
    [HttpPost("backups/verify")]
    public async Task<ActionResult<BackupVerificationResponse>> VerifyBackup([FromBody] VerifyBackupRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            await _auditLoggingService.LogUserActionAsync(
                "BACKUP_VERIFICATION_INITIATED",
                userId,
                $"Backup file: {request.BackupFileName}");

            var backupPath = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "backups", 
                request.BackupFileName);

            var isValid = await _backupService.VerifyBackupIntegrityAsync(backupPath);

            var response = new BackupVerificationResponse
            {
                FileName = request.BackupFileName,
                IsValid = isValid,
                VerifiedAt = DateTime.UtcNow
            };

            await _auditLoggingService.LogUserActionAsync(
                "BACKUP_VERIFICATION_COMPLETED",
                userId,
                $"Backup file: {request.BackupFileName}, Valid: {isValid}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify backup");
            return StatusCode(500, new { message = "Failed to verify backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Cleanup old backups
    /// </summary>
    [HttpPost("backups/cleanup")]
    public async Task<ActionResult> CleanupOldBackups([FromBody] CleanupBackupsRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            await _auditLoggingService.LogUserActionAsync(
                "BACKUP_CLEANUP_INITIATED",
                userId,
                $"Retention days: {request.RetentionDays}");

            await _backupService.CleanupOldBackupsAsync(request.RetentionDays);

            return Ok(new { message = "Backup cleanup completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
            return StatusCode(500, new { message = "Failed to cleanup old backups", error = ex.Message });
        }
    }

    /// <summary>
    /// Test PCI compliance validation
    /// </summary>
    [HttpPost("pci/validate-card")]
    public async Task<ActionResult<CardValidationResponse>> ValidateCard([FromBody] CardValidationRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Log the validation attempt (without card details)
            await _auditLoggingService.LogUserActionAsync(
                "PCI_CARD_VALIDATION_TEST",
                userId,
                "Card validation test performed");

            var isValid = _pciComplianceService.ValidateCardDataFormat(
                request.CardNumber, 
                request.ExpiryDate, 
                request.Cvv);

            var maskedCardNumber = _pciComplianceService.MaskCreditCardNumber(request.CardNumber);

            var response = new CardValidationResponse
            {
                IsValid = isValid,
                MaskedCardNumber = maskedCardNumber,
                ValidationPerformed = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate card data");
            return StatusCode(500, new { message = "Failed to validate card data", error = ex.Message });
        }
    }

    /// <summary>
    /// Log a security event for testing
    /// </summary>
    [HttpPost("audit/security-event")]
    public async Task<ActionResult> LogSecurityEvent([FromBody] SecurityEventRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            await _auditLoggingService.LogSecurityEventAsync(
                request.EventType,
                request.Description,
                userId,
                ipAddress,
                userAgent);

            return Ok(new { message = "Security event logged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event");
            return StatusCode(500, new { message = "Failed to log security event", error = ex.Message });
        }
    }
}

// Request/Response models
public class CreateBackupRequest
{
    public string? BackupName { get; set; }
}

public class BackupCreationResponse
{
    public string BackupPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Success { get; set; }
}

public class VerifyBackupRequest
{
    public string BackupFileName { get; set; } = string.Empty;
}

public class BackupVerificationResponse
{
    public string FileName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class CleanupBackupsRequest
{
    public int RetentionDays { get; set; } = 30;
}

public class CardValidationRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}

public class CardValidationResponse
{
    public bool IsValid { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public DateTime ValidationPerformed { get; set; }
}

public class SecurityEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}