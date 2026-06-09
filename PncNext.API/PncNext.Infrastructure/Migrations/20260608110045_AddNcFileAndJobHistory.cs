using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PncNext.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNcFileAndJobHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiskInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiskBarcode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaterialType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UsedAreaLayout = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiskInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NcFileInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsValidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NcFileInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NcFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiskId = table.Column<int>(type: "INTEGER", nullable: false),
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
                        name: "FK_JobHistories_DiskInventories_DiskId",
                        column: x => x.DiskId,
                        principalTable: "DiskInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobHistories_NcFileInventories_NcFileId",
                        column: x => x.NcFileId,
                        principalTable: "NcFileInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_DiskId",
                table: "JobHistories",
                column: "DiskId");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistories_NcFileId",
                table: "JobHistories",
                column: "NcFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobHistories");

            migrationBuilder.DropTable(
                name: "DiskInventories");

            migrationBuilder.DropTable(
                name: "NcFileInventories");
        }
    }
}
