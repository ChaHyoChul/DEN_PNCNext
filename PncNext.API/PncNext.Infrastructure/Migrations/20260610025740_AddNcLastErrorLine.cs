using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNcLastErrorLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastErrorLine",
                table: "NcFileInventories",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastErrorLine",
                table: "NcFileInventories");
        }
    }
}
