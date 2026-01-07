#nullable enable
using FluentAssertions;
using MiniDrive.Storage;
using Xunit;

namespace MiniDrive.StorageUnitTests;

/// <summary>
/// Unit tests for StorageOptions configuration class.
/// Tests default values, validation, and configuration behavior.
/// </summary>
public class StorageOptionsTests
{
    [Fact]
    public void StorageOptions_initializes_with_default_values()
    {
        // Arrange & Act
        var options = new StorageOptions();

        // Assert
        options.BasePath.Should().NotBeNullOrEmpty();
        options.MaxFileSizeBytes.Should().BeGreaterThan(0);
        options.AllowedExtensions.Should().NotBeNull();
    }

    [Fact]
    public void StorageOptions_can_set_custom_base_path()
    {
        // Arrange
        var customPath = "/custom/storage/path";
        var options = new StorageOptions { BasePath = customPath };

        // Act & Assert
        options.BasePath.Should().Be(customPath);
    }

    [Fact]
    public void StorageOptions_can_set_custom_max_file_size()
    {
        // Arrange
        var maxSize = 50 * 1024 * 1024; // 50 MB
        var options = new StorageOptions { MaxFileSizeBytes = maxSize };

        // Act & Assert
        options.MaxFileSizeBytes.Should().Be(maxSize);
    }

    [Fact]
    public void StorageOptions_can_set_allowed_extensions()
    {
        // Arrange
        var options = new StorageOptions();
        options.AllowedExtensions.Add(".pdf");
        options.AllowedExtensions.Add(".docx");

        // Act & Assert
        options.AllowedExtensions.Should().Contain(".pdf");
        options.AllowedExtensions.Should().Contain(".docx");
        options.AllowedExtensions.Should().HaveCount(2);
    }

    [Fact]
    public void StorageOptions_allowed_extensions_collection_behavior()
    {
        // Arrange
        var options = new StorageOptions();

        // Act - Add extensions
        options.AllowedExtensions.Add(".txt");
        options.AllowedExtensions.Add(".pdf");
        options.AllowedExtensions.Add(".docx");

        // Assert
        options.AllowedExtensions.Should().HaveCountGreaterThanOrEqualTo(3);
        options.AllowedExtensions.Should().Contain(".txt");
        options.AllowedExtensions.Should().Contain(".pdf");
        options.AllowedExtensions.Should().Contain(".docx");
    }

    [Fact]
    public void StorageOptions_empty_allowed_extensions_means_all_allowed()
    {
        // Arrange
        var options = new StorageOptions();

        // Act & Assert
        options.AllowedExtensions.Should().BeEmpty(); // Empty = no restrictions
    }
}
