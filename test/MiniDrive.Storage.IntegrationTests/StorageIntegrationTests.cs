using FluentAssertions;
using Microsoft.Extensions.Options;
using MiniDrive.Storage;
using Xunit;

namespace MiniDrive.Storage.IntegrationTests;

public class StorageIntegrationTests : IDisposable
{
    private readonly string _storagePath;
    private readonly IFileStorage _fileStorage;

    public StorageIntegrationTests()
    {
        _storagePath = Path.Combine(Path.GetTempPath(), $"MiniDrive_Storage_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_storagePath);

        var options = Options.Create(new StorageOptions
        {
            BasePath = _storagePath,
            MaxFileSizeBytes = 100 * 1024 * 1024, // 100MB
            AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".txt", ".pdf", ".jpg", ".png"
            }
        });
        _fileStorage = new LocalFileStorage(options);
    }

    [Fact]
    public async Task SaveAsync_ValidFile_SavesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Test file content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var result = await _fileStorage.SaveAsync(stream, fileName);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var fullPath = _fileStorage.GetFullPath(result);
        File.Exists(fullPath).Should().BeTrue();
        
        var savedContent = await File.ReadAllTextAsync(fullPath);
        savedContent.Should().Be(content);
    }

    [Fact]
    public async Task ReadAsync_ExistingFile_ReturnsStream()
    {
        // Arrange
        var fileName = "read_test.txt";
        var content = "Content to read";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var savedPath = await _fileStorage.SaveAsync(stream, fileName);

        // Act
        var readStream = await _fileStorage.GetAsync(savedPath);

        // Assert
        readStream.Should().NotBeNull();
        using var reader = new StreamReader(readStream);
        var readContent = await reader.ReadToEndAsync();
        readContent.Should().Be(content);
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_DeletesFile()
    {
        // Arrange
        var fileName = "delete_test.txt";
        var content = "Content to delete";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var savedPath = await _fileStorage.SaveAsync(stream, fileName);
        var filePath = _fileStorage.GetFullPath(savedPath);
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _fileStorage.DeleteAsync(savedPath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task GetAsync_NonExistentFile_ThrowsException()
    {
        // Arrange
        var nonExistentPath = "non_existent_file.txt";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await _fileStorage.GetAsync(nonExistentPath));
    }

    [Fact]
    public async Task SaveAsync_LargeFile_ThrowsException()
    {
        // Arrange
        var fileName = "large_file.txt";
        var largeContent = new byte[101 * 1024 * 1024]; // 101MB, exceeds 100MB limit
        var stream = new MemoryStream(largeContent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _fileStorage.SaveAsync(stream, fileName));
    }

    public void Dispose()
    {
        if (Directory.Exists(_storagePath))
        {
            Directory.Delete(_storagePath, true);
        }
    }
}

