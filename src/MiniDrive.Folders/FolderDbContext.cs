using Microsoft.EntityFrameworkCore;
using MiniDrive.Folders.Entities;

namespace MiniDrive.Folders;

/// <summary>
/// Entity Framework Core DbContext for folders.
/// </summary>
public class FolderDbContext : DbContext
{
    public FolderDbContext(DbContextOptions<FolderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Folder> Folders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ParentFolderId);
            entity.HasIndex(e => new { e.OwnerId, e.ParentFolderId, e.IsDeleted });
            entity.HasIndex(e => e.IsDeleted);
        });
    }
}
