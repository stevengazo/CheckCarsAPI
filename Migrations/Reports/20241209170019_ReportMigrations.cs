using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckCarsAPI.Migrations.Reports
{
    /// <inheritdoc />
    public partial class ReportMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Plate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Width = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    Lenght = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.CarId);
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
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                });

            migrationBuilder.CreateTable(
                name: "CarsService",
                columns: table => new
                {
                    CarServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarsService", x => x.CarServiceId);
                    table.ForeignKey(
                        name: "FK_CarsService_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "CarId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrashReports",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    DateOfCrash = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CrashDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CrashedParts = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrashReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_CrashReports_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "CarId");
                });

            migrationBuilder.CreateTable(
                name: "EntryExitReports",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_EntryExitReports_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "CarId");
                });

            migrationBuilder.CreateTable(
                name: "IssueReports",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_IssueReports_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "CarId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarsService_CarId",
                table: "CarsService",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CrashReports_CarId",
                table: "CrashReports",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_EntryExitReports_CarId",
                table: "EntryExitReports",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueReports_CarId",
                table: "IssueReports",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ReportId",
                table: "Photos",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarsService");

            migrationBuilder.DropTable(
                name: "CrashReports");

            migrationBuilder.DropTable(
                name: "EntryExitReports");

            migrationBuilder.DropTable(
                name: "IssueReports");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
