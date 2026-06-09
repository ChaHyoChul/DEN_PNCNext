using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDiskInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiskBarcode",
                table: "DiskInventories",
                newName: "DiskID");

            migrationBuilder.AddColumn<double>(
                name: "Thickness",
                table: "DiskInventories",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Thickness",
                table: "DiskInventories");

            migrationBuilder.RenameColumn(
                name: "DiskID",
                table: "DiskInventories",
                newName: "DiskBarcode");
        }
    }
}
