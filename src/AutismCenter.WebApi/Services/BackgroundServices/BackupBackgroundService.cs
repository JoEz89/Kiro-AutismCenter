using NCrontab;

namespace AutismCenter.WebApi.Services.BackgroundServices;

public class BackupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupBackgroundService> _logger;
    private readonly string _cronExpression;
    private readonly bool _autoBackupEnabled;

    public BackupBackgroundService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<BackupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
        _cronExpression = configuration["Backup:AutoBackupSchedule"] ?? "0 2 * * *"; // Default: 2 AM daily
        _autoBackupEnabled = configuration.GetValue<bool>("Backup:AutoBackupEnabled", true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_autoBackupEnabled)
        {
            _logger.LogInformation("Automatic backup is disabled");
            return;
        }

        _logger.LogInformation("Backup background service started with schedule: {CronExpression}", _cronExpression);

        try
        {
            var crontabSchedule = CrontabSchedule.Parse(_cronExpression);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                var delay = nextRun - DateTime.UtcNow;

                _logger.LogInformation("Next backup scheduled for: {NextRun} UTC", nextRun);

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await PerformScheduledBackupAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Backup background service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in backup background service");
        }
    }

    private async Task PerformScheduledBackupAsync()
    {
        try
        {
            _logger.LogInformation("Starting scheduled backup");

            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IDataBackupService>();
            var auditLoggingService = scope.ServiceProvider.GetRequiredService<IAuditLoggingService>();

            // Create database backup
            var databaseBackupPath = await backupService.CreateDatabaseBackupAsync();
            _logger.LogInformation("Database backup created: {BackupPath}", databaseBackupPath);

            // Create application data backup
            var appDataBackupPath = await backupService.CreateApplicationDataBackupAsync();
            _logger.LogInformation("Application data backup created: {BackupPath}", appDataBackupPath);

            // Cleanup old backups
            var retentionDays = _configuration.GetValue<int>("Backup:RetentionDays", 30);
            await backupService.CleanupOldBackupsAsync(retentionDays);

            await auditLoggingService.LogSystemEventAsync(
                "SCHEDULED_BACKUP_COMPLETED",
                "Scheduled backup completed successfully",
                $"Database backup: {Path.GetFileName(databaseBackupPath)}, App data backup: {Path.GetFileName(appDataBackupPath)}");

            _logger.LogInformation("Scheduled backup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled backup failed");
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var auditLoggingService = scope.ServiceProvider.GetRequiredService<IAuditLoggingService>();
                await auditLoggingService.LogSystemEventAsync(
                    "SCHEDULED_BACKUP_FAILED",
                    "Scheduled backup failed",
                    ex.Message);
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Failed to log backup failure to audit log");
            }
        }
    }
}