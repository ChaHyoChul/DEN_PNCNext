using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCommStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaudRate",
                table: "MotionControllerConfigs");

            migrationBuilder.DropColumn(
                name: "ComPort",
                table: "MotionControllerConfigs");

            migrationBuilder.DropColumn(
                name: "CommType",
                table: "MotionControllerConfigs");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "MotionControllerConfigs");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "MotionControllerConfigs");

            migrationBuilder.CreateTable(
                name: "CommItemConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MotionControllerConfigId = table.Column<int>(type: "INTEGER", nullable: false),
                    CommType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    ComPort = table.Column<string>(type: "TEXT", nullable: true),
                    BaudRate = table.Column<int>(type: "INTEGER", nullable: true),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommItemConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommItemConfigs_MotionControllerConfigs_MotionControllerConfigId",
                        column: x => x.MotionControllerConfigId,
                        principalTable: "MotionControllerConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommItemConfigs_MotionControllerConfigId",
                table: "CommItemConfigs",
                column: "MotionControllerConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommItemConfigs");

            migrationBuilder.AddColumn<int>(
                name: "BaudRate",
                table: "MotionControllerConfigs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComPort",
                table: "MotionControllerConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommType",
                table: "MotionControllerConfigs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "MotionControllerConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "MotionControllerConfigs",
                type: "INTEGER",
                nullable: true);
        }
    }
}
