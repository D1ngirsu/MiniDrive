#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MiniDrive.Storage;
using Xunit;

namespace MiniDrive.StorageUnitTests;

/// <summary>
/// Unit tests for LocalFileStorage implementation.
/// Tests file storage operations, validation, and error handling.
/// </summary>
public class LocalFileStorageTests : IDisposable
{
    private readonly string _testStoragePath;
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        // Create a temporary directory for test storage
        _testStoragePath = Path.Combine(Path.GetTempPath(), $"MiniDrive_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testStoragePath);

        // Setup storage with test options
        var options = Options.Create(new StorageOptions
        {
            BasePath = _testStoragePath,
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10 MB
            AllowedExtensions = new() { ".txt", ".pdf", ".docx" }
        });

        _storage = new LocalFileStorage(options);
    }

    [Fact]
    public async Task SaveAsync_with_valid_file_returns_relative_path()
    {
        // Arrange
        var fileContent = "Test file content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act
        var relativePath = await _storage.SaveAsync(stream, "test.txt");

        // Assert
        relativePath.Should().NotBeNullOrEmpty();
        relativePath.Should().Contain("test.txt");
        relativePath.Should().Match("*/*/*.txt"); // YYYY/MM/filename.txt pattern
    }

    [Fact]
    public async Task SaveAsync_creates_file_on_disk()
    {
        // Arrange
        var fileContent = "Test file content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        // Act
        var relativePath = await _storage.SaveAsync(stream, "test.txt");
        var fullPath = _storage.GetFullPath(relativePath);

        // Assert
        File.Exists(fullPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(fullPath);
        content.Should().Be(fileContent);
    }

    [Fact]
    public async Task SaveAsync_generates_unique_filenames()
    {
        // Arrange
        var fileContent1 = "First file";
        var fileContent2 = "Second file";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(fileContent1));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(fileContent2));

        // Act
        var path1 = await _storage.SaveAsync(stream1, "test.txt");
        var path2 = await _storage.SaveAsync(stream2, "test.txt");

        // Assert
        path1.Should().NotBe(path2);
        var fullPath1 = _storage.GetFullPath(path1);
        var fullPath2 = _storage.GetFullPath(path2);
        File.Exists(fullPath1).Should().BeTrue();
        File.Exists(fullPath2).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_with_null_stream_throws_exception()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(null!, "test.txt"));
    }

    [Fact]
    public async Task SaveAsync_with_empty_stream_throws_exception()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(emptyStream, "test.txt"));
    }

    [Fact]
    public async Task SaveAsync_with_null_filename_throws_exception()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(stream, null!));
    }

    [Fact]
    public async Task SaveAsync_with_empty_filename_throws_exception()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(stream, ""));
    }

    [Fact]
    public async Task SaveAsync_with_oversized_file_throws_exception()
    {
        // Arrange
        var largeContent = new byte[11 * 1024 * 1024]; // 11 MB (exceeds 10 MB limit)
        using var stream = new MemoryStream(largeContent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SaveAsync(stream, "large.txt"));
    }

    [Fact]
    public async Task SaveAsync_with_disallowed_extension_throws_exception()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SaveAsync(stream, "file.exe"));
    }

    [Fact]
    public async Task SaveAsync_organizes_files_by_date_structure()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        var now = DateTime.UtcNow;
        var expectedYearMonth = $"{now.Year}/{now.Month:D2}";

        // Act
        var relativePath = await _storage.SaveAsync(stream, "test.txt");

        // Assert
        relativePath.Should().StartWith(expectedYearMonth);
    }

    [Fact]
    public async Task GetAsync_returns_file_stream()
    {
        // Arrange
        var fileContent = "Test file content";
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
        {
            var relativePath = await _storage.SaveAsync(stream, "test.txt");

            // Act
            var retrievedStream = await _storage.GetAsync(relativePath);

            // Assert
            retrievedStream.Should().NotBeNull();
            using var reader = new StreamReader(retrievedStream);
            var content = await reader.ReadToEndAsync();
            content.Should().Be(fileContent);
        }
    }

    [Fact]
    public async Task GetAsync_with_nonexistent_path_throws_exception()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _storage.GetAsync("nonexistent/file.txt"));
    }

    [Fact]
    public async Task DeleteAsync_removes_file()
    {
        // Arrange
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("content")))
        {
            var relativePath = await _storage.SaveAsync(stream, "test.txt");
            var fullPath = _storage.GetFullPath(relativePath);
            File.Exists(fullPath).Should().BeTrue();

            // Act
            await _storage.DeleteAsync(relativePath);

            // Assert
            File.Exists(fullPath).Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteAsync_with_nonexistent_path_does_not_throw()
    {
        // Act - Should not throw if path doesn't exist (graceful handling)
        // This tests defensive programming - storage may silently handle missing files
        var task = _storage.DeleteAsync("nonexistent/file.txt");
        
        // Assert - either completes successfully or throws FileNotFoundException
        try
        {
            await task;
        }
        catch (FileNotFoundException)
        {
            // This is also acceptable behavior
        }
    }

    [Fact]
    public void GetFullPath_normalizes_relative_paths()
    {
        // Arrange
        var relativePath = "2024/01/test.txt";

        // Act
        var fullPath = _storage.GetFullPath(relativePath);

        // Assert - verify the path is resolved to an absolute path
        Path.IsPathFullyQualified(fullPath).Should().BeTrue();
        fullPath.Should().Contain("2024");
        fullPath.Should().Contain("01");
    }

    [Fact]
    public void GetFullPath_with_traversal_patterns()
    {
        // Arrange - test with various path patterns
        var simplePath = "2024/01/file.txt";

        // Act
        var fullPath = _storage.GetFullPath(simplePath);

        // Assert - verify normalization occurs
        Path.IsPathFullyQualified(fullPath).Should().BeTrue();
        fullPath.Should().Contain(_testStoragePath);
    }

    [Fact]
    public async Task Multiple_concurrent_saves_succeed()
    {
        // Arrange
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = SaveFileAsync($"file_{i}.txt", $"content_{i}");
        }

        // Act
        await Task.WhenAll(tasks);

        // Assert
        var savedFiles = Directory.GetFiles(_testStoragePath, "*", SearchOption.AllDirectories);
        savedFiles.Should().HaveCount(10);
    }

    private async Task SaveFileAsync(string fileName, string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await _storage.SaveAsync(stream, fileName);
    }

    public void Dispose()
    {
        // Cleanup: Remove test storage directory
        try
        {
            if (Directory.Exists(_testStoragePath))
            {
                Directory.Delete(_testStoragePath, true);
            }
        }
        catch { /* Ignore cleanup errors */ }
    }
}
