using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniDrive.Sharing.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharedWithUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Permission = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPublicShare = table.Column<bool>(type: "bit", nullable: false),
                    ShareToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxDownloads = table.Column<int>(type: "int", nullable: true),
                    CurrentDownloads = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shares_IsDeleted",
                table: "Shares",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_OwnerId",
                table: "Shares",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_OwnerId_IsDeleted",
                table: "Shares",
                columns: new[] { "OwnerId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ResourceId_ResourceType",
                table: "Shares",
                columns: new[] { "ResourceId", "ResourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ShareToken",
                table: "Shares",
                column: "ShareToken",
                unique: true,
                filter: "[ShareToken] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_SharedWithUserId",
                table: "Shares",
                column: "SharedWithUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_SharedWithUserId_IsActive_IsDeleted",
                table: "Shares",
                columns: new[] { "SharedWithUserId", "IsActive", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shares");
        }
    }
}
