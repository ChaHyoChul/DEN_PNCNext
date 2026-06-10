using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiskInventories",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiskId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiskName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaterialType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Thickness = table.Column<double>(type: "REAL", nullable: false),
                    UsedAreaLayout = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskInventories", x => x.Seq);
                });

            migrationBuilder.CreateTable(
                name: "MotionControllerConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ControllerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ControllerType = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotionControllerConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NcFileInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DiskSeq = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetDiskName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsValidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NcFileInventories", x => x.Id);
                });

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
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProtocolProvider = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "JobHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NcFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiskSeq = table.Column<int>(type: "INTEGER", nullable: false),
                    JobStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorCode = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorLineNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentJobId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobHistories_DiskInventories_DiskSeq",
                        column: x => x.DiskSeq,
                        principalTable: "DiskInventories",
                        principalColumn: "Seq",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobHistories_NcFileInventories_NcFileId",
                        column: x => x.NcFileId,
                        principalTable: "NcFileInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommItemConfigs_MotionControllerConfigId",
                table: "CommItemConfigs",
                column: "MotionControllerConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_DiskInventories_DiskId",
                table: "DiskInventories",
                column: "DiskId",
                unique: true,
                filter: "\"IsDeleted\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DiskInventories_DiskName",
                table: "DiskInventories",
                column: "DiskName",
                unique: true,
                filter: "\"IsDeleted\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_DiskSeq",
                table: "JobHistories",
                column: "DiskSeq");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_NcFileId",
                table: "JobHistories",
                column: "NcFileId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOptionConfigs_MotionControllerConfigId",
                table: "MachineOptionConfigs",
                column: "MotionControllerConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommItemConfigs");

            migrationBuilder.DropTable(
                name: "JobHistories");

            migrationBuilder.DropTable(
                name: "MachineOptionConfigs");

            migrationBuilder.DropTable(
                name: "DiskInventories");

            migrationBuilder.DropTable(
                name: "NcFileInventories");

            migrationBuilder.DropTable(
                name: "MotionControllerConfigs");
        }
    }
}
