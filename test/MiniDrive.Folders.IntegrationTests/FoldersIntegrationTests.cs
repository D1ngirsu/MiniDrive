using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Folders;
using MiniDrive.Folders.Entities;
using MiniDrive.Folders.Repositories;
using MiniDrive.Folders.Services;
using Xunit;

namespace MiniDrive.Folders.IntegrationTests;

public class FoldersIntegrationTests : IDisposable
{
    private readonly FolderDbContext _context;
    private readonly FolderRepository _folderRepository;
    private readonly FolderService _folderService;
    private readonly string _dbFile;

    public FoldersIntegrationTests()
    {
        _dbFile = $"{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<FolderDbContext>()
            .UseSqlite($"Data Source={_dbFile}")
            .Options;

        _context = new FolderDbContext(options);
        _context.Database.EnsureCreated();

        _folderRepository = new FolderRepository(_context);
        _folderService = new FolderService(_folderRepository);
    }

    [Fact]
    public async Task CreateFolderAsync_ValidFolder_CreatesFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "Test Folder";

        // Act
        var result = await _folderService.CreateFolderAsync(name, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(name);
        result.Value.OwnerId.Should().Be(ownerId);

        // Verify folder is persisted
        var folder = await _folderRepository.GetByIdAsync(result.Value.Id);
        folder.Should().NotBeNull();
        folder!.Name.Should().Be(name);
    }

    [Fact]
    public async Task CreateFolderAsync_WithParent_CreatesNestedFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var parentResult = await _folderService.CreateFolderAsync("Parent", ownerId);
        var parentId = parentResult.Value!.Id;

        // Act
        var result = await _folderService.CreateFolderAsync("Child", ownerId, parentId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value!.ParentFolderId.Should().Be(parentId);
    }

    [Fact]
    public async Task CreateFolderAsync_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var name = "Duplicate Folder";
        await _folderService.CreateFolderAsync(name, ownerId);

        // Act
        var result = await _folderService.CreateFolderAsync(name, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetFolderAsync_ExistingFolder_ReturnsFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var createResult = await _folderService.CreateFolderAsync("Test Folder", ownerId);
        var folderId = createResult.Value!.Id;

        // Act
        var result = await _folderService.GetFolderAsync(folderId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test Folder");
    }

    [Fact]
    public async Task GetFolderAsync_NonExistentFolder_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var folderId = Guid.NewGuid();

        // Act
        var result = await _folderService.GetFolderAsync(folderId, ownerId);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateFolderAsync_ExistingFolder_UpdatesMetadata()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var createResult = await _folderService.CreateFolderAsync("Original Name", ownerId);
        var folderId = createResult.Value!.Id;

        // Act
        var result = await _folderService.UpdateFolderAsync(
            folderId, ownerId, "Updated Name", "Updated Description", null, null);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value!.Name.Should().Be("Updated Name");
        result.Value.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteFolderAsync_ExistingFolder_DeletesFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var createResult = await _folderService.CreateFolderAsync("To Delete", ownerId);
        var folderId = createResult.Value!.Id;

        // Act
        var result = await _folderService.DeleteFolderAsync(folderId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify folder is soft deleted (GetByIdAndOwnerAsync filters out deleted items)
        var folder = await _folderRepository.GetByIdAndOwnerAsync(folderId, ownerId);
        folder.Should().BeNull();
    }

    [Fact]
    public async Task ListFoldersAsync_WithFolders_ReturnsAllFolders()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        await _folderService.CreateFolderAsync("Folder 1", ownerId);
        await _folderService.CreateFolderAsync("Folder 2", ownerId);

        // Act
        var result = await _folderService.ListFoldersAsync(ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetFolderPathAsync_WithNestedFolders_ReturnsPath()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var rootResult = await _folderService.CreateFolderAsync("Root", ownerId);
        var rootId = rootResult.Value!.Id;
        
        var childResult = await _folderService.CreateFolderAsync("Child", ownerId, rootId);
        var childId = childResult.Value!.Id;

        // Act
        var result = await _folderService.GetFolderPathAsync(childId, ownerId);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count().Should().BeGreaterThanOrEqualTo(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        // Clean up SQLite file
        if (File.Exists(_dbFile))
        {
            File.Delete(_dbFile);
        }
    }
}

