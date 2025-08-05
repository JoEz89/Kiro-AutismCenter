using System.Diagnostics;
using System.IO.Compression;

namespace AutismCenter.WebApi.Services;

public interface IDataBackupService
{
    Task<string> CreateDatabaseBackupAsync(string backupName = null);
    Task<bool> RestoreDatabaseBackupAsync(string backupPath);
    Task<string> CreateApplicationDataBackupAsync();
    Task<bool> VerifyBackupIntegrityAsync(string backupPath);
    Task CleanupOldBackupsAsync(int retentionDays = 30);
    Task<List<BackupInfo>> GetAvailableBackupsAsync();
}

public class DataBackupService : IDataBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataBackupService> _logger;
    private readonly IAuditLoggingService _auditLoggingService;
    private readonly string _backupDirectory;
    private readonly string _connectionString;

    public DataBackupService(
        IConfiguration configuration,
        ILogger<DataBackupService> logger,
        IAuditLoggingService auditLoggingService)
    {
        _configuration = configuration;
        _logger = logger;
        _auditLoggingService = auditLoggingService;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
        _backupDirectory = configuration["Backup:Directory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "backups");
        
        // Ensure backup directory exists
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<string> CreateDatabaseBackupAsync(string backupName = null)
    {
        try
        {
            backupName ??= $"autism_center_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var backupPath = Path.Combine(_backupDirectory, $"{backupName}.sql");

            _logger.LogInformation("Starting database backup: {BackupName}", backupName);

            // Extract connection details
            var connectionParts = ParseConnectionString(_connectionString);
            
            // Create PostgreSQL dump command
            var dumpCommand = $"pg_dump -h {connectionParts.Host} -p {connectionParts.Port} -U {connectionParts.Username} -d {connectionParts.Database} -f \"{backupPath}\" --verbose --clean --if-exists --create";

            // Set environment variable for password
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $"-h {connectionParts.Host} -p {connectionParts.Port} -U {connectionParts.Username} -d {connectionParts.Database} -f \"{backupPath}\" --verbose --clean --if-exists --create",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            processStartInfo.EnvironmentVariables["PGPASSWORD"] = connectionParts.Password;

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start pg_dump process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Database backup failed. Error: {Error}", error);
                throw new InvalidOperationException($"Database backup failed: {error}");
            }

            // Compress the backup file
            var compressedPath = await CompressBackupAsync(backupPath);
            
            // Delete the uncompressed file
            File.Delete(backupPath);

            // Verify backup integrity
            var isValid = await VerifyBackupIntegrityAsync(compressedPath);
            if (!isValid)
            {
                throw new InvalidOperationException("Backup integrity verification failed");
            }

            _logger.LogInformation("Database backup completed successfully: {BackupPath}", compressedPath);
            
            await _auditLoggingService.LogSystemEventAsync(
                "DATABASE_BACKUP_CREATED",
                $"Database backup created successfully: {Path.GetFileName(compressedPath)}",
                $"Size: {new FileInfo(compressedPath).Length} bytes");

            return compressedPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            await _auditLoggingService.LogSystemEventAsync(
                "DATABASE_BACKUP_FAILED",
                "Database backup creation failed",
                ex.Message);
            throw;
        }
    }

    public async Task<bool> RestoreDatabaseBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                return false;
            }

            _logger.LogInformation("Starting database restore from: {BackupPath}", backupPath);

            // Decompress if needed
            var sqlFilePath = backupPath;
            if (Path.GetExtension(backupPath) == ".gz")
            {
                sqlFilePath = await DecompressBackupAsync(backupPath);
            }

            var connectionParts = ParseConnectionString(_connectionString);

            // Create PostgreSQL restore command
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "psql",
                Arguments = $"-h {connectionParts.Host} -p {connectionParts.Port} -U {connectionParts.Username} -d {connectionParts.Database} -f \"{sqlFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            processStartInfo.EnvironmentVariables["PGPASSWORD"] = connectionParts.Password;

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start psql process");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            // Clean up decompressed file if it was created
            if (sqlFilePath != backupPath && File.Exists(sqlFilePath))
            {
                File.Delete(sqlFilePath);
            }

            if (process.ExitCode != 0)
            {
                _logger.LogError("Database restore failed. Error: {Error}", error);
                await _auditLoggingService.LogSystemEventAsync(
                    "DATABASE_RESTORE_FAILED",
                    "Database restore failed",
                    error);
                return false;
            }

            _logger.LogInformation("Database restore completed successfully");
            await _auditLoggingService.LogSystemEventAsync(
                "DATABASE_RESTORE_COMPLETED",
                $"Database restored from backup: {Path.GetFileName(backupPath)}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database backup");
            await _auditLoggingService.LogSystemEventAsync(
                "DATABASE_RESTORE_FAILED",
                "Database restore failed with exception",
                ex.Message);
            return false;
        }
    }

    public async Task<string> CreateApplicationDataBackupAsync()
    {
        try
        {
            var backupName = $"app_data_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            var backupPath = Path.Combine(_backupDirectory, backupName);

            _logger.LogInformation("Starting application data backup: {BackupName}", backupName);

            using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);

            // Backup configuration files
            var configFiles = new[] { "appsettings.json", "appsettings.Production.json" };
            foreach (var configFile in configFiles)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), configFile);
                if (File.Exists(filePath))
                {
                    archive.CreateEntryFromFile(filePath, $"config/{configFile}");
                }
            }

            // Backup data protection keys
            var keysDirectory = Path.Combine(Directory.GetCurrentDirectory(), "keys");
            if (Directory.Exists(keysDirectory))
            {
                foreach (var keyFile in Directory.GetFiles(keysDirectory))
                {
                    archive.CreateEntryFromFile(keyFile, $"keys/{Path.GetFileName(keyFile)}");
                }
            }

            // Backup logs (last 7 days)
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (Directory.Exists(logsDirectory))
            {
                var recentLogs = Directory.GetFiles(logsDirectory)
                    .Where(f => File.GetCreationTime(f) > DateTime.Now.AddDays(-7));

                foreach (var logFile in recentLogs)
                {
                    archive.CreateEntryFromFile(logFile, $"logs/{Path.GetFileName(logFile)}");
                }
            }

            _logger.LogInformation("Application data backup completed: {BackupPath}", backupPath);
            
            await _auditLoggingService.LogSystemEventAsync(
                "APPLICATION_DATA_BACKUP_CREATED",
                $"Application data backup created: {backupName}",
                $"Size: {new FileInfo(backupPath).Length} bytes");

            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create application data backup");
            await _auditLoggingService.LogSystemEventAsync(
                "APPLICATION_DATA_BACKUP_FAILED",
                "Application data backup failed",
                ex.Message);
            throw;
        }
    }

    public async Task<bool> VerifyBackupIntegrityAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return false;

            // For compressed files, try to decompress and verify
            if (Path.GetExtension(backupPath) == ".gz")
            {
                using var fileStream = File.OpenRead(backupPath);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                
                // Try to read the first few lines to verify it's valid
                var firstLine = await reader.ReadLineAsync();
                return !string.IsNullOrEmpty(firstLine);
            }

            // For ZIP files, verify the archive can be opened
            if (Path.GetExtension(backupPath) == ".zip")
            {
                using var archive = ZipFile.OpenRead(backupPath);
                return archive.Entries.Count > 0;
            }

            // For SQL files, check if file is readable and contains SQL content
            if (Path.GetExtension(backupPath) == ".sql")
            {
                var content = await File.ReadAllTextAsync(backupPath);
                return !string.IsNullOrEmpty(content) && content.Contains("--");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify backup integrity: {BackupPath}", backupPath);
            return false;
        }
    }

    public async Task CleanupOldBackupsAsync(int retentionDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var backupFiles = Directory.GetFiles(_backupDirectory);
            var deletedCount = 0;

            foreach (var backupFile in backupFiles)
            {
                var fileInfo = new FileInfo(backupFile);
                if (fileInfo.CreationTimeUtc < cutoffDate)
                {
                    File.Delete(backupFile);
                    deletedCount++;
                    _logger.LogInformation("Deleted old backup: {BackupFile}", Path.GetFileName(backupFile));
                }
            }

            await _auditLoggingService.LogSystemEventAsync(
                "BACKUP_CLEANUP_COMPLETED",
                $"Cleaned up {deletedCount} old backup files",
                $"Retention period: {retentionDays} days");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old backups");
            await _auditLoggingService.LogSystemEventAsync(
                "BACKUP_CLEANUP_FAILED",
                "Backup cleanup failed",
                ex.Message);
        }
    }

    public async Task<List<BackupInfo>> GetAvailableBackupsAsync()
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory);
            var backups = new List<BackupInfo>();

            foreach (var backupFile in backupFiles)
            {
                var fileInfo = new FileInfo(backupFile);
                var isValid = await VerifyBackupIntegrityAsync(backupFile);

                backups.Add(new BackupInfo
                {
                    FileName = Path.GetFileName(backupFile),
                    FilePath = backupFile,
                    Size = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTimeUtc,
                    IsValid = isValid,
                    Type = GetBackupType(backupFile)
                });
            }

            return backups.OrderByDescending(b => b.CreatedDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available backups");
            return new List<BackupInfo>();
        }
    }

    private async Task<string> CompressBackupAsync(string filePath)
    {
        var compressedPath = filePath + ".gz";
        
        using var originalFileStream = File.OpenRead(filePath);
        using var compressedFileStream = File.Create(compressedPath);
        using var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
        
        await originalFileStream.CopyToAsync(compressionStream);
        
        return compressedPath;
    }

    private async Task<string> DecompressBackupAsync(string compressedPath)
    {
        var decompressedPath = compressedPath.Replace(".gz", "");
        
        using var compressedFileStream = File.OpenRead(compressedPath);
        using var decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
        using var decompressedFileStream = File.Create(decompressedPath);
        
        await decompressionStream.CopyToAsync(decompressedFileStream);
        
        return decompressedPath;
    }

    private ConnectionDetails ParseConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';');
        var details = new ConnectionDetails();

        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length != 2) continue;

            var key = keyValue[0].Trim().ToLowerInvariant();
            var value = keyValue[1].Trim();

            switch (key)
            {
                case "host":
                    details.Host = value;
                    break;
                case "port":
                    details.Port = value;
                    break;
                case "database":
                    details.Database = value;
                    break;
                case "username":
                    details.Username = value;
                    break;
                case "password":
                    details.Password = value;
                    break;
            }
        }

        return details;
    }

    private string GetBackupType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();

        if (fileName.Contains("app_data"))
            return "Application Data";
        
        if (extension == ".sql" || extension == ".gz")
            return "Database";
        
        if (extension == ".zip")
            return "Application Data";

        return "Unknown";
    }

    private class ConnectionDetails
    {
        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5432";
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsValid { get; set; }
    public string Type { get; set; } = string.Empty;
}