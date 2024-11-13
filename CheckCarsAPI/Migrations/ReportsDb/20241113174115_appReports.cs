using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckCarsAPI.Migrations.ReportsDb
{
    /// <inheritdoc />
    public partial class appReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ReportSequence");

            migrationBuilder.CreateTable(
                name: "CrashReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [ReportSequence]"),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    DateOfCrash = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CrashDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CrashedParts = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrashReports", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "EntryExitReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [ReportSequence]"),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    mileage = table.Column<long>(type: "bigint", nullable: false),
                    FuelLevel = table.Column<double>(type: "float", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasChargerUSB = table.Column<bool>(type: "bit", nullable: false),
                    HasQuickPass = table.Column<bool>(type: "bit", nullable: false),
                    HasPhoneSupport = table.Column<bool>(type: "bit", nullable: false),
                    TiresState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasSpareTire = table.Column<bool>(type: "bit", nullable: false),
                    HasEmergencyKit = table.Column<bool>(type: "bit", nullable: false),
                    PaintState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MecanicState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OilLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InteriorsState = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryExitReports", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "IssueReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [ReportSequence]"),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReports", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTaken = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ReportId",
                table: "Photos",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrashReports");

            migrationBuilder.DropTable(
                name: "EntryExitReports");

            migrationBuilder.DropTable(
                name: "IssueReports");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropSequence(
                name: "ReportSequence");
        }
    }
}
