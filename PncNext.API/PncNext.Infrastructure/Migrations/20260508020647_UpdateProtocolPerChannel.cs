using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProtocolPerChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProtocolProvider",
                table: "MotionControllerConfigs");

            migrationBuilder.AddColumn<string>(
                name: "ProtocolProvider",
                table: "CommItemConfigs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProtocolProvider",
                table: "CommItemConfigs");

            migrationBuilder.AddColumn<string>(
                name: "ProtocolProvider",
                table: "MotionControllerConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
