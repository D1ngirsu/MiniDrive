using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Files;
using MiniDrive.Files.Entities;
using MiniDrive.Files.Repositories;
using Xunit;

namespace MiniDrive.Files.Tests;

public class FileRepositoryTests : IDisposable
{
    private readonly FileDbContext _context;
    private readonly FileRepository _repository;

    public FileRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FileDbContext(options);
        _repository = new FileRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ValidFile_CreatesFile()
    {
        // Arrange
        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            OwnerId = Guid.NewGuid()
        };

        // Act
        var result = await _repository.CreateAsync(file);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(file.Id);
        var saved = await _context.Files.FindAsync(file.Id);
        saved.Should().NotBeNull();
        saved!.FileName.Should().Be("test.txt");
    }

    [Fact]
    public async Task GetByIdAndOwnerAsync_ExistingFile_ReturnsFile()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            OwnerId = ownerId,
            IsDeleted = false
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAndOwnerAsync(file.Id, ownerId);

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test.txt");
    }

    [Fact]
    public async Task GetByIdAndOwnerAsync_DeletedFile_ReturnsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            OwnerId = ownerId,
            IsDeleted = true
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAndOwnerAsync(file.Id, ownerId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOwnerAsync_WithFolderId_ReturnsFilesInFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var file1 = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "file1.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/file1.txt",
            OwnerId = ownerId,
            FolderId = folderId,
            IsDeleted = false
        };
        var file2 = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "file2.txt",
            ContentType = "text/plain",
            SizeBytes = 2048,
            StoragePath = "2024/01/file2.txt",
            OwnerId = ownerId,
            FolderId = null,
            IsDeleted = false
        };
        _context.Files.AddRange(file1, file2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOwnerAsync(ownerId, folderId);

        // Assert
        result.Should().HaveCount(1);
        result.First().FileName.Should().Be("file1.txt");
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_MarksAsDeleted()
    {
        // Arrange
        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            OwnerId = Guid.NewGuid(),
            IsDeleted = false
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(file.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Files.FindAsync(file.Id);
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task HardDeleteAsync_ExistingFile_RemovesFile()
    {
        // Arrange
        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "test.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/test.txt",
            OwnerId = Guid.NewGuid()
        };
        _context.Files.Add(file);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HardDeleteAsync(file.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Files.FindAsync(file.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetTotalSizeByOwnerAsync_ReturnsSumOfFileSizes()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var file1 = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "file1.txt",
            ContentType = "text/plain",
            SizeBytes = 1024,
            StoragePath = "2024/01/file1.txt",
            OwnerId = ownerId,
            IsDeleted = false
        };
        var file2 = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "file2.txt",
            ContentType = "text/plain",
            SizeBytes = 2048,
            StoragePath = "2024/01/file2.txt",
            OwnerId = ownerId,
            IsDeleted = false
        };
        var file3 = new FileEntry
        {
            Id = Guid.NewGuid(),
            FileName = "file3.txt",
            ContentType = "text/plain",
            SizeBytes = 4096,
            StoragePath = "2024/01/file3.txt",
            OwnerId = ownerId,
            IsDeleted = true // Should not be counted
        };
        _context.Files.AddRange(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalSizeByOwnerAsync(ownerId);

        // Assert
        result.Should().Be(1024 + 2048);
    }
}

