using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniDrive.Sharing.Migrations
{
    /// <inheritdoc />
    public partial class SharingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MiniDrive.Sharing.Entities.Share", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedNever()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CurrentDownloads")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpiresAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPublicShare")
                        .HasColumnType("bit");

                    b.Property<int?>("MaxDownloads")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("PasswordHash")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Permission")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<Guid>("ResourceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResourceType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("ShareToken")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("SharedWithUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("IsDeleted");

                    b.HasIndex("ResourceId", "ResourceType");

                    b.HasIndex("ShareToken")
                        .IsUnique()
                        .HasFilter("[ShareToken] IS NOT NULL AND [IsDeleted] = 0");

                    b.HasIndex("SharedWithUserId");

                    b.HasIndex("OwnerId", "IsDeleted");

                    b.HasIndex("SharedWithUserId", "IsActive", "IsDeleted");

                    b.ToTable("Shares");
                });
#pragma warning restore 612, 618
        }

        
    }
}
