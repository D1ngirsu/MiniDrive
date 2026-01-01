using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniDrive.Audit;
using MiniDrive.Audit.Repositories;
using MiniDrive.Audit.Services;
using MiniDrive.Files;
using MiniDrive.Files.Entities;
using MiniDrive.Files.Repositories;
using MiniDrive.Files.Services;
using MiniDrive.Quota;
using MiniDrive.Quota.Repositories;
using MiniDrive.Quota.Services;
using MiniDrive.Storage;
using Xunit;

namespace MiniDrive.Files.IntegrationTests;

public class FilesIntegrationTests : IDisposable
{
    private readonly FileDbContext _fileContext;
    private readonly AuditDbContext _auditContext;
    private readonly QuotaDbContext _quotaContext;
    private readonly FileRepository _fileRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IQuotaService _quotaService;
    private readonly IAuditService _auditService;
    private readonly FileService _fileService;
    private readonly string _storagePath;
    private readonly string _fileDbFile;
    private readonly string _auditDbFile;
    private readonly string _quotaDbFile;

    public FilesIntegrationTests()
    {
        // Setup file database
        _fileDbFile = $"{Guid.NewGuid()}.db";
        var fileOptions = new DbContextOptionsBuilder<FileDbContext>()
            .UseSqlite($"Data Source={_fileDbFile}")
            .Options;
        _fileContext = new FileDbContext(fileOptions);
        _fileContext.Database.EnsureCreated();

        // Setup audit database
        _auditDbFile = $"{Guid.NewGuid()}.db";
        var auditOptions = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlite($"Data Source={_auditDbFile}")
            .Options;
        _auditContext = new AuditDbContext(auditOptions);
        _auditContext.Database.EnsureCreated();

        // Setup quota database
        _quotaDbFile = $"{Guid.NewGuid()}.db";
        var quotaOptions = new DbContextOptionsBuilder<QuotaDbContext>()
            .UseSqlite($"Data Source={_quotaDbFile}")
            .Options;
        _quotaContext = new QuotaDbContext(quotaOptions);
        _quotaContext.Database.EnsureCreated();

        // Setup storage
        _storagePath = Path.Combine(Path.GetTempPath(), $"MiniDrive_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_storagePath);
        var storageOptions = Options.Create(new StorageOptions
        {
            BasePath = _storagePath,
            MaxFileSizeBytes = 100 * 1024 * 1024
        });
        _fileStorage = new LocalFileStorage(storageOptions);

        // Setup repositories and services
        _fileRepository = new FileRepository(_fileContext);
        _quotaService = new QuotaService(new QuotaRepository(_quotaContext));
        _auditService = new AuditService(new AuditRepository(_auditContext));
        _fileService = new FileService(_fileRepository, _fileStorage, _quotaService, _auditService);
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_CreatesFileEntry()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var result = await _fileService.UploadFileAsync(
            stream, "test.txt", "text/plain", ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().Be("test.txt");
        result.Value.OwnerId.Should().Be(ownerId);

        // Verify file is persisted
        var file = await _fileRepository.GetByIdAsync(result.Value.Id);
        file.Should().NotBeNull();
        file!.FileName.Should().Be("test.txt");
    }

    [Fact]
    public async Task UploadFileAsync_WithFolderId_AssociatesWithFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var result = await _fileService.UploadFileAsync(
            stream, "test.txt", "text/plain", ownerId, folderId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value!.FolderId.Should().Be(folderId);
    }

    [Fact]
    public async Task DownloadFileAsync_ExistingFile_ReturnsFileStream()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var uploadResult = await _fileService.UploadFileAsync(
            stream, "test.txt", "text/plain", ownerId);
        var fileId = uploadResult.Value!.Id;

        // Act
        var result = await _fileService.DownloadFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.File.FileName.Should().Be("test.txt");
        result.Value.Content.Should().NotBeNull();
        
        // Dispose the stream to release file lock
        result.Value.Content.Dispose();
    }

    [Fact]
    public async Task DownloadFileAsync_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        // Act
        var result = await _fileService.DownloadFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFileAsync_ExistingFile_UpdatesMetadata()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var uploadResult = await _fileService.UploadFileAsync(
            stream, "test.txt", "text/plain", ownerId);
        var fileId = uploadResult.Value!.Id;

        // Act
        var result = await _fileService.UpdateFileAsync(
            fileId, ownerId, "updated.txt", "Updated description", null);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value!.FileName.Should().Be("updated.txt");
        result.Value.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_SoftDeletesFile()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var uploadResult = await _fileService.UploadFileAsync(
            stream, "test.txt", "text/plain", ownerId);
        var fileId = uploadResult.Value!.Id;

        // Act
        var result = await _fileService.DeleteFileAsync(fileId, ownerId, "127.0.0.1", "TestAgent");

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify file is soft deleted (GetByIdAndOwnerAsync filters out deleted items)
        var file = await _fileRepository.GetByIdAndOwnerAsync(fileId, ownerId);
        file.Should().BeNull(); // Soft deleted files are not returned
    }

    [Fact]
    public async Task ListFilesAsync_WithFiles_ReturnsAllFiles()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var content = "Test file content";
        
        var stream1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _fileService.UploadFileAsync(stream1, "file1.txt", "text/plain", ownerId);
        
        var stream2 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await _fileService.UploadFileAsync(stream2, "file2.txt", "text/plain", ownerId);

        // Act
        var result = await _fileService.ListFilesAsync(ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    public void Dispose()
    {
        _fileContext.Database.EnsureDeleted();
        _fileContext.Dispose();
        _auditContext.Database.EnsureDeleted();
        _auditContext.Dispose();
        _quotaContext.Database.EnsureDeleted();
        _quotaContext.Dispose();
        
        // Clean up SQLite files
        if (File.Exists(_fileDbFile))
        {
            File.Delete(_fileDbFile);
        }
        if (File.Exists(_auditDbFile))
        {
            File.Delete(_auditDbFile);
        }
        if (File.Exists(_quotaDbFile))
        {
            File.Delete(_quotaDbFile);
        }
        
        if (Directory.Exists(_storagePath))
        {
            Directory.Delete(_storagePath, true);
        }
    }
}

