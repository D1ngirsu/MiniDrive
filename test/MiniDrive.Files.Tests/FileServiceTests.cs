using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Audit.Services;
using MiniDrive.Files;
using MiniDrive.Files.Entities;
using MiniDrive.Files.Repositories;
using MiniDrive.Files.Services;
using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Services;
using MiniDrive.Storage;
using Moq;
using Xunit;

namespace MiniDrive.Files.Tests;

public class FileServiceTests : IDisposable
{
    private readonly FileDbContext _fileContext;
    private readonly FileRepository _fileRepository;
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IQuotaService> _quotaServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _fileContext = new FileDbContext(options);
        _fileRepository = new FileRepository(_fileContext);
        _fileStorageMock = new Mock<IFileStorage>();
        _quotaServiceMock = new Mock<IQuotaService>();
        _auditServiceMock = new Mock<IAuditService>();
        _fileService = new FileService(
            _fileRepository,
            _fileStorageMock.Object,
            _quotaServiceMock.Object,
            _auditServiceMock.Object);
    }

    public void Dispose()
    {
        _fileContext.Dispose();
    }

    [Fact]
    public async Task UploadFileAsync_NullStream_ReturnsFailure()
    {
        // Arrange
        Stream? stream = null;
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _fileService.UploadFileAsync(stream!, "test.txt", "text/plain", ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("null or empty");
        _auditServiceMock.Verify(x => x.LogActionAsync(
            ownerId, "FileUpload", "File", It.IsAny<string>(), false, It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_EmptyStream_ReturnsFailure()
    {
        // Arrange
        var stream = new MemoryStream();
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _fileService.UploadFileAsync(stream, "test.txt", "text/plain", ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("null or empty");
    }

    [Fact]
    public async Task UploadFileAsync_QuotaExceeded_ReturnsFailure()
    {
        // Arrange
        var content = "Hello, World!";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var ownerId = Guid.NewGuid();
        _quotaServiceMock.Setup(x => x.CanUploadAsync(ownerId, stream.Length)).ReturnsAsync(false);
        _quotaServiceMock.Setup(x => x.GetQuotaAsync(ownerId)).ReturnsAsync((UserQuota?)null);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "test.txt", "text/plain", ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("quota");
        _quotaServiceMock.Verify(x => x.CanUploadAsync(ownerId, It.IsAny<long>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var content = "Hello, World!";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var ownerId = Guid.NewGuid();
        var storagePath = "2024/01/test.txt";
        var fileEntry = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = stream.Length,
            StoragePath = storagePath,
            OwnerId = ownerId
        };

        _quotaServiceMock.Setup(x => x.CanUploadAsync(ownerId, It.IsAny<long>())).ReturnsAsync(true);
        _fileStorageMock.Setup(x => x.SaveAsync(It.IsAny<Stream>(), "test.txt")).ReturnsAsync(storagePath);
        _quotaServiceMock.Setup(x => x.IncreaseAsync(ownerId, It.IsAny<long>())).ReturnsAsync(true);

        // Act
        var result = await _fileService.UploadFileAsync(stream, "test.txt", "text/plain", ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FileName.Should().Be("test.txt");
        _fileStorageMock.Verify(x => x.SaveAsync(It.IsAny<Stream>(), "test.txt"), Times.Once);
        _quotaServiceMock.Verify(x => x.IncreaseAsync(ownerId, It.IsAny<long>()), Times.Once);
        _auditServiceMock.Verify(x => x.LogActionAsync(
            ownerId, "FileUpload", "File", It.IsAny<string>(), true, It.IsAny<string>(), 
            null, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        
        // Verify file was saved to database
        var savedFile = await _fileContext.Files.FindAsync(result.Value.Id);
        savedFile.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadFileAsync_FileNotFound_ReturnsFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _fileService.DownloadFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DownloadFileAsync_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var fileEntry = new FileEntry
        {
            Id = fileId,
            FileName = "test.txt",
            StoragePath = "2024/01/test.txt",
            OwnerId = ownerId,
            ContentType = "text/plain",
            SizeBytes = 1024,
            Extension = ".txt"
        };
        _fileContext.Files.Add(fileEntry);
        await _fileContext.SaveChangesAsync();
        
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));
        _fileStorageMock.Setup(x => x.GetAsync(fileEntry.StoragePath)).ReturnsAsync(stream);

        // Act
        var result = await _fileService.DownloadFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.File.Should().NotBeNull();
        result.Value.Content.Should().NotBeNull();
        _fileStorageMock.Verify(x => x.GetAsync(fileEntry.StoragePath), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_FileNotFound_ReturnsFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _fileService.DeleteFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var fileEntry = new FileEntry
        {
            Id = fileId,
            FileName = "test.txt",
            OwnerId = ownerId,
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            Extension = ".txt"
        };
        _fileContext.Files.Add(fileEntry);
        await _fileContext.SaveChangesAsync();

        // Act
        var result = await _fileService.DeleteFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        var deleted = await _fileContext.Files.FindAsync(fileId);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task PermanentlyDeleteFileAsync_ValidFile_DecreasesQuota()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var fileSize = 1024L;
        var fileEntry = new FileEntry
        {
            Id = fileId,
            FileName = "test.txt",
            StoragePath = "2024/01/test.txt",
            SizeBytes = fileSize,
            OwnerId = ownerId,
            ContentType = "text/plain",
            Extension = ".txt"
        };
        _fileContext.Files.Add(fileEntry);
        await _fileContext.SaveChangesAsync();

        _fileStorageMock.Setup(x => x.DeleteAsync(fileEntry.StoragePath)).Returns(Task.CompletedTask);
        _quotaServiceMock.Setup(x => x.DecreaseAsync(ownerId, fileSize)).ReturnsAsync(true);

        // Act
        var result = await _fileService.PermanentlyDeleteFileAsync(fileId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        _fileStorageMock.Verify(x => x.DeleteAsync(fileEntry.StoragePath), Times.Once);
        _quotaServiceMock.Verify(x => x.DecreaseAsync(ownerId, fileSize), Times.Once);
        
        // Verify file was hard deleted
        var deleted = await _fileContext.Files.FindAsync(fileId);
        deleted.Should().BeNull();
    }
}

