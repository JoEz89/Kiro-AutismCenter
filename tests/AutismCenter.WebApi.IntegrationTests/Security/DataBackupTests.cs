using AutismCenter.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AutismCenter.WebApi.IntegrationTests.Security;

public class DataBackupTests : IDisposable
{
    private readonly IDataBackupService _backupService;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<DataBackupService>> _loggerMock;
    private readonly Mock<IAuditLoggingService> _auditLoggingServiceMock;
    private readonly string _testBackupDirectory;

    public DataBackupTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DataBackupService>>();
        _auditLoggingServiceMock = new Mock<IAuditLoggingService>();
        
        _testBackupDirectory = Path.Combine(Path.GetTempPath(), "test_backups", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testBackupDirectory);

        // Setup configuration mocks
        _configurationMock.Setup(x => x["Backup:Directory"]).Returns(_testBackupDirectory);
        _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
            .Returns("Host=localhost;Database=test_db;Username=test_user;Password=test_pass");

        _backupService = new DataBackupService(
            _configurationMock.Object,
            _loggerMock.Object,
            _auditLoggingServiceMock.Object);
    }

    [Fact]
    public async Task CreateApplicationDataBackupAsync_ShouldCreateBackupFile()
    {
        // Arrange
        var testConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        await File.WriteAllTextAsync(testConfigFile, "{ \"test\": \"config\" }");

        try
        {
            // Act
            var backupPath = await _backupService.CreateApplicationDataBackupAsync();

            // Assert
            Assert.True(File.Exists(backupPath));
            Assert.True(Path.GetExtension(backupPath) == ".zip");
            
            // Verify audit logging
            _auditLoggingServiceMock.Verify(x => x.LogSystemEventAsync(
                "APPLICATION_DATA_BACKUP_CREATED",
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }
        finally
        {
            // Cleanup
            if (File.Exists(testConfigFile))
                File.Delete(testConfigFile);
        }
    }

    [Fact]
    public async Task VerifyBackupIntegrityAsync_WithValidZipFile_ShouldReturnTrue()
    {
        // Arrange
        var testZipPath = Path.Combine(_testBackupDirectory, "test_backup.zip");
        using (var archive = System.IO.Compression.ZipFile.Open(testZipPath, System.IO.Compression.ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("test.txt");
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync("Test content");
        }

        // Act
        var result = await _backupService.VerifyBackupIntegrityAsync(testZipPath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyBackupIntegrityAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testBackupDirectory, "nonexistent.zip");

        // Act
        var result = await _backupService.VerifyBackupIntegrityAsync(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAvailableBackupsAsync_ShouldReturnBackupList()
    {
        // Arrange
        var testBackup1 = Path.Combine(_testBackupDirectory, "backup1.zip");
        var testBackup2 = Path.Combine(_testBackupDirectory, "app_data_backup_20240101.zip");
        
        await File.WriteAllTextAsync(testBackup1, "test content");
        await File.WriteAllTextAsync(testBackup2, "test content");

        // Act
        var backups = await _backupService.GetAvailableBackupsAsync();

        // Assert
        Assert.NotEmpty(backups);
        Assert.Contains(backups, b => b.FileName == "backup1.zip");
        Assert.Contains(backups, b => b.FileName == "app_data_backup_20240101.zip");
        Assert.Contains(backups, b => b.Type == "Application Data");
    }

    [Fact]
    public async Task CleanupOldBackupsAsync_ShouldRemoveOldFiles()
    {
        // Arrange
        var oldBackup = Path.Combine(_testBackupDirectory, "old_backup.zip");
        var recentBackup = Path.Combine(_testBackupDirectory, "recent_backup.zip");
        
        await File.WriteAllTextAsync(oldBackup, "old content");
        await File.WriteAllTextAsync(recentBackup, "recent content");
        
        // Make the old backup appear old by setting its creation time
        File.SetCreationTime(oldBackup, DateTime.UtcNow.AddDays(-35));

        // Act
        await _backupService.CleanupOldBackupsAsync(30);

        // Assert
        Assert.False(File.Exists(oldBackup));
        Assert.True(File.Exists(recentBackup));
        
        // Verify audit logging
        _auditLoggingServiceMock.Verify(x => x.LogSystemEventAsync(
            "BACKUP_CLEANUP_COMPLETED",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("backup_20240101.sql", "Database")]
    [InlineData("backup_20240101.sql.gz", "Database")]
    [InlineData("app_data_backup_20240101.zip", "Application Data")]
    [InlineData("unknown_file.txt", "Unknown")]
    public async Task GetAvailableBackupsAsync_ShouldIdentifyBackupTypes(string fileName, string expectedType)
    {
        // Arrange
        var testBackup = Path.Combine(_testBackupDirectory, fileName);
        await File.WriteAllTextAsync(testBackup, "test content");

        // Act
        var backups = await _backupService.GetAvailableBackupsAsync();

        // Assert
        var backup = backups.FirstOrDefault(b => b.FileName == fileName);
        Assert.NotNull(backup);
        Assert.Equal(expectedType, backup.Type);
    }

    [Fact]
    public async Task CreateApplicationDataBackupAsync_OnException_ShouldLogFailure()
    {
        // Arrange
        var invalidBackupService = new DataBackupService(
            _configurationMock.Object,
            _loggerMock.Object,
            _auditLoggingServiceMock.Object);

        // Setup configuration to point to invalid directory
        _configurationMock.Setup(x => x["Backup:Directory"]).Returns("/invalid/path/that/does/not/exist");

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => invalidBackupService.CreateApplicationDataBackupAsync());

        // Verify failure was logged
        _auditLoggingServiceMock.Verify(x => x.LogSystemEventAsync(
            "APPLICATION_DATA_BACKUP_FAILED",
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testBackupDirectory))
        {
            Directory.Delete(_testBackupDirectory, true);
        }
    }
}