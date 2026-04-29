using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MotionControllerConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ControllerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CommType = table.Column<string>(type: "TEXT", nullable: false),
                    ProtocolProvider = table.Column<string>(type: "TEXT", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    ComPort = table.Column<string>(type: "TEXT", nullable: true),
                    BaudRate = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotionControllerConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MachineOptionConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MotionControllerConfigId = table.Column<int>(type: "INTEGER", nullable: false),
                    IOMapType = table.Column<string>(type: "TEXT", nullable: false),
                    AxisCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ToolPocketType = table.Column<string>(type: "TEXT", nullable: false),
                    HasAutoloader = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasDustCollector = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasWaterPump = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpindleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    HasPurgeAir = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasAirBlow = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineOptionConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineOptionConfigs_MotionControllerConfigs_MotionControllerConfigId",
                        column: x => x.MotionControllerConfigId,
                        principalTable: "MotionControllerConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineOptionConfigs_MotionControllerConfigId",
                table: "MachineOptionConfigs",
                column: "MotionControllerConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineOptionConfigs");

            migrationBuilder.DropTable(
                name: "MotionControllerConfigs");
        }
    }
}
