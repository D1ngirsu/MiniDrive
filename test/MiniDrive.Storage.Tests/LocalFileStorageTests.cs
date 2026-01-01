using FluentAssertions;
using Microsoft.Extensions.Options;
using MiniDrive.Storage;
using Xunit;

namespace MiniDrive.Storage.Tests;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = Options.Create(new StorageOptions
        {
            BasePath = _testBasePath,
            MaxFileSizeBytes = 100 * 1024 * 1024, // 100MB
            AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt", ".pdf", ".jpg" }
        });
        _storage = new LocalFileStorage(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [Fact]
    public async Task SaveAsync_ValidFile_ReturnsRelativePath()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var result = await _storage.SaveAsync(stream, fileName);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(fileName);
        var fullPath = Path.Combine(_testBasePath, result);
        File.Exists(fullPath).Should().BeTrue();
        var savedContent = await File.ReadAllTextAsync(fullPath);
        savedContent.Should().Be(content);
    }

    [Fact]
    public async Task SaveAsync_NullStream_ThrowsArgumentException()
    {
        // Arrange
        Stream? stream = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(stream!, "test.txt"));
    }

    [Fact]
    public async Task SaveAsync_EmptyStream_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(stream, "test.txt"));
    }

    [Fact]
    public async Task SaveAsync_EmptyFileName_ThrowsArgumentException()
    {
        // Arrange
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.SaveAsync(stream, ""));
    }

    [Fact]
    public async Task SaveAsync_FileExceedsMaxSize_ThrowsInvalidOperationException()
    {
        // Arrange
        var fileName = "large.txt";
        var largeContent = new byte[101 * 1024 * 1024]; // 101MB
        using var stream = new MemoryStream(largeContent);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SaveAsync(stream, fileName));
    }

    [Fact]
    public async Task SaveAsync_DisallowedExtension_ThrowsInvalidOperationException()
    {
        // Arrange
        var fileName = "test.exe";
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SaveAsync(stream, fileName));
    }

    [Fact]
    public async Task SaveAsync_GeneratesUniqueFileName()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello, World!";
        using var stream1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        using var stream2 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var path1 = await _storage.SaveAsync(stream1, fileName);
        var path2 = await _storage.SaveAsync(stream2, fileName);

        // Assert
        path1.Should().NotBe(path2);
    }

    [Fact]
    public async Task SaveAsync_SanitizesFileName()
    {
        // Arrange
        var fileName = "test<>file|name.txt";
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        // Act
        var result = await _storage.SaveAsync(stream, fileName);

        // Assert
        result.Should().NotContain("<");
        result.Should().NotContain(">");
        result.Should().NotContain("|");
    }

    [Fact]
    public async Task GetAsync_ValidPath_ReturnsStream()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello, World!";
        using var writeStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var savedPath = await _storage.SaveAsync(writeStream, fileName);

        // Act
        using var readStream = await _storage.GetAsync(savedPath);
        using var reader = new StreamReader(readStream);
        var readContent = await reader.ReadToEndAsync();

        // Assert
        readContent.Should().Be(content);
    }

    [Fact]
    public async Task GetAsync_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.GetAsync(""));
    }

    [Fact]
    public async Task GetAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _storage.GetAsync("nonexistent/file.txt"));
    }

    [Fact]
    public async Task GetAsync_PathTraversal_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _storage.GetAsync("../../../etc/passwd"));
    }

    [Fact]
    public async Task DeleteAsync_ValidPath_DeletesFile()
    {
        // Arrange
        var fileName = "test.txt";
        var content = "Hello, World!";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var savedPath = await _storage.SaveAsync(stream, fileName);
        var fullPath = Path.Combine(_testBasePath, savedPath);
        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _storage.DeleteAsync(savedPath);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.DeleteAsync(""));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentFile_DoesNotThrow()
    {
        // Act
        await _storage.DeleteAsync("nonexistent/file.txt");

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public async Task DeleteAsync_PathTraversal_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _storage.DeleteAsync("../../../etc/passwd"));
    }

    [Fact]
    public void GetFullPath_ValidPath_ReturnsFullPath()
    {
        // Arrange
        var relativePath = "2024/01/test.txt";

        // Act
        var fullPath = _storage.GetFullPath(relativePath);

        // Assert
        fullPath.Should().StartWith(_testBasePath);
        // Normalize path separators for cross-platform compatibility
        var normalizedFullPath = fullPath.Replace('\\', '/');
        var normalizedRelativePath = relativePath.Replace('\\', '/');
        normalizedFullPath.Should().Contain(normalizedRelativePath);
    }

    [Fact]
    public void GetFullPath_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _storage.GetFullPath(""));
    }

    [Fact]
    public void GetFullPath_PathTraversal_ThrowsUnauthorizedAccessException()
    {
        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() => _storage.GetFullPath("../../../etc/passwd"));
    }
}

