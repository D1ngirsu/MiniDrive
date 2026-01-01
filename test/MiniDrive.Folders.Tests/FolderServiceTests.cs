using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Folders;
using MiniDrive.Folders.Entities;
using MiniDrive.Folders.Repositories;
using MiniDrive.Folders.Services;
using Xunit;

namespace MiniDrive.Folders.Tests;

public class FolderServiceTests : IDisposable
{
    private readonly FolderDbContext _folderContext;
    private readonly FolderRepository _folderRepository;
    private readonly FolderService _folderService;

    public FolderServiceTests()
    {
        var options = new DbContextOptionsBuilder<FolderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _folderContext = new FolderDbContext(options);
        _folderRepository = new FolderRepository(_folderContext);
        _folderService = new FolderService(_folderRepository);
    }

    public void Dispose()
    {
        _folderContext.Dispose();
    }

    [Fact]
    public async Task CreateFolderAsync_EmptyName_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _folderService.CreateFolderAsync("", ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("name");
    }

    [Fact]
    public async Task CreateFolderAsync_InvalidParent_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        // Act
        var result = await _folderService.CreateFolderAsync("NewFolder", ownerId, parentId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Parent");
    }

    [Fact]
    public async Task CreateFolderAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var existingFolder = new Folder 
        { 
            Id = Guid.NewGuid(), 
            Name = "ExistingFolder", 
            OwnerId = ownerId 
        };
        _folderContext.Folders.Add(existingFolder);
        await _folderContext.SaveChangesAsync();

        // Act
        var result = await _folderService.CreateFolderAsync("ExistingFolder", ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateFolderAsync_ValidFolder_ReturnsSuccess()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _folderService.CreateFolderAsync("NewFolder", ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("NewFolder");
        var saved = await _folderContext.Folders.FindAsync(result.Value.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFolderAsync_NotFound_ReturnsFailure()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var result = await _folderService.GetFolderAsync(folderId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFolderAsync_MoveIntoItself_ReturnsFailure()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var folder = new Folder 
        { 
            Id = folderId, 
            Name = "Folder", 
            OwnerId = ownerId 
        };
        _folderContext.Folders.Add(folder);
        await _folderContext.SaveChangesAsync();

        // Act
        var result = await _folderService.UpdateFolderAsync(folderId, ownerId, parentFolderId: folderId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("itself");
    }

    [Fact]
    public async Task DeleteFolderAsync_WithChildren_ReturnsFailure()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var folder = new Folder 
        { 
            Id = folderId, 
            Name = "Folder", 
            OwnerId = ownerId 
        };
        var child = new Folder 
        { 
            Id = Guid.NewGuid(), 
            Name = "Child", 
            OwnerId = ownerId, 
            ParentFolderId = folderId 
        };
        _folderContext.Folders.AddRange(folder, child);
        await _folderContext.SaveChangesAsync();

        // Act
        var result = await _folderService.DeleteFolderAsync(folderId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("subfolders");
    }
}

