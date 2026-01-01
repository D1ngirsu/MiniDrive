using Microsoft.EntityFrameworkCore;
using MiniDrive.Files.Entities;

namespace MiniDrive.Files;

/// <summary>
/// Entity Framework Core DbContext for file entries.
/// </summary>
public class FileDbContext : DbContext
{
    public FileDbContext(DbContextOptions<FileDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileEntry> Files { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Extension).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.SizeBytes).IsRequired();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => new { e.OwnerId, e.FolderId, e.IsDeleted });
            entity.HasIndex(e => e.IsDeleted);
        });
    }
}
