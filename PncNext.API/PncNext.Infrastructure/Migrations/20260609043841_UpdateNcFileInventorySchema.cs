using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNcFileInventorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NcFileInventories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetDiskId",
                table: "NcFileInventories",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "NcFileInventories");

            migrationBuilder.DropColumn(
                name: "TargetDiskId",
                table: "NcFileInventories");
        }
    }
}
