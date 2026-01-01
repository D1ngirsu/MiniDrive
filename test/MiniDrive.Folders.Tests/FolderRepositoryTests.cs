using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MiniDrive.Folders;
using MiniDrive.Folders.Entities;
using MiniDrive.Folders.Repositories;
using Xunit;

namespace MiniDrive.Folders.Tests;

public class FolderRepositoryTests : IDisposable
{
    private readonly FolderDbContext _context;
    private readonly FolderRepository _repository;

    public FolderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FolderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FolderDbContext(options);
        _repository = new FolderRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ValidFolder_CreatesFolder()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "TestFolder",
            OwnerId = Guid.NewGuid()
        };

        // Act
        var result = await _repository.CreateAsync(folder);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(folder.Id);
        var saved = await _context.Folders.FindAsync(folder.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("TestFolder");
    }

    [Fact]
    public async Task GetByIdAndOwnerAsync_ExistingFolder_ReturnsFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "TestFolder",
            OwnerId = ownerId,
            IsDeleted = false
        };
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAndOwnerAsync(folder.Id, ownerId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestFolder");
    }

    [Fact]
    public async Task GetByNameAndParentAsync_ExistingFolder_ReturnsFolder()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "TestFolder",
            OwnerId = ownerId,
            ParentFolderId = null,
            IsDeleted = false
        };
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAndParentAsync("TestFolder", ownerId, null);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestFolder");
    }

    [Fact]
    public async Task GetByOwnerAsync_WithParentId_ReturnsFoldersInParent()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var folder1 = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Folder1",
            OwnerId = ownerId,
            ParentFolderId = parentId,
            IsDeleted = false
        };
        var folder2 = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "Folder2",
            OwnerId = ownerId,
            ParentFolderId = null,
            IsDeleted = false
        };
        _context.Folders.AddRange(folder1, folder2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOwnerAsync(ownerId, parentId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Folder1");
    }

    [Fact]
    public async Task DeleteAsync_ExistingFolder_MarksAsDeleted()
    {
        // Arrange
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = "TestFolder",
            OwnerId = Guid.NewGuid(),
            IsDeleted = false
        };
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(folder.Id);

        // Assert
        result.Should().BeTrue();
        var deleted = await _context.Folders.FindAsync(folder.Id);
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAtUtc.Should().NotBeNull();
    }
}

