using Microsoft.EntityFrameworkCore;
using MiniDrive.Quota.Entities;

namespace MiniDrive.Quota;

/// <summary>
/// Entity Framework Core DbContext for user quotas.
/// </summary>
public class QuotaDbContext : DbContext
{
    public QuotaDbContext(DbContextOptions<QuotaDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserQuota> UserQuotas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserQuota>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UsedBytes).IsRequired();
            entity.Property(e => e.LimitBytes).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
            
            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }
}
