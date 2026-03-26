using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniDrive.Sharing.Migrations
{
    /// <inheritdoc />
    public partial class _20260326_SharingModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shares_IsDeleted",
                table: "Shares");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shares_IsDeleted",
                table: "Shares",
                column: "IsDeleted");
        }
    }
}
