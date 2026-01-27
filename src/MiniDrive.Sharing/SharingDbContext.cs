using Microsoft.EntityFrameworkCore;
using MiniDrive.Sharing.Entities;

namespace MiniDrive.Sharing;

/// <summary>
/// Entity Framework Core DbContext for share management.
/// </summary>
public class SharingDbContext : DbContext
{
    public SharingDbContext(DbContextOptions<SharingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Share> Shares { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.ResourceId).IsRequired();
            entity.Property(e => e.ResourceType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.Permission).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ShareToken).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();

            // Indexes for efficient querying
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.SharedWithUserId);
            entity.HasIndex(e => new { e.OwnerId, e.IsDeleted });
            entity.HasIndex(e => new { e.ResourceId, e.ResourceType });
            entity.HasIndex(e => e.ShareToken).IsUnique().HasFilter("[ShareToken] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.SharedWithUserId, e.IsActive, e.IsDeleted });
        });
    }
}
