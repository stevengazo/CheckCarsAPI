using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckCarsAPI.Migrations.CarsContext
{
    /// <inheritdoc />
    public partial class AddDynamicForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DynamicReportId",
                table: "Photos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlotKey",
                table: "Photos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DynamicReports",
                columns: table => new
                {
                    ReportId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormDefinitionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CarPlate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CarId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_DynamicReports_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "CarId");
                });

            migrationBuilder.CreateTable(
                name: "FormDefinitions",
                columns: table => new
                {
                    FormDefinitionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhotoSlots = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDefinitions", x => x.FormDefinitionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_DynamicReportId",
                table: "Photos",
                column: "DynamicReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicReports_CarId",
                table: "DynamicReports",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_DynamicReports_DynamicReportId",
                table: "Photos",
                column: "DynamicReportId",
                principalTable: "DynamicReports",
                principalColumn: "ReportId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_DynamicReports_DynamicReportId",
                table: "Photos");

            migrationBuilder.DropTable(
                name: "DynamicReports");

            migrationBuilder.DropTable(
                name: "FormDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Photos_DynamicReportId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "DynamicReportId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "SlotKey",
                table: "Photos");
        }
    }
}
