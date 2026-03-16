using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacialRecognitionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeLocationTableForCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRadiusMeters",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OfficeLatitude",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OfficeLongitude",
                table: "Employees");

            migrationBuilder.CreateTable(
                name: "OfficeLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    AllowedRadiusMeters = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeLocations_IsActive",
                table: "OfficeLocations",
                column: "IsActive");

            migrationBuilder.InsertData(
                table: "OfficeLocations",
                columns: new[] { "Id", "Name", "Latitude", "Longitude", "AllowedRadiusMeters", "IsActive" },
                values: new object[] { new Guid("8C21D8A2-8265-4B91-B62A-6F66A3A7C8DD"), "Main Office", 24.8950233m, 67.1521653m, 100, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OfficeLocations",
                keyColumn: "Id",
                keyValue: new Guid("8C21D8A2-8265-4B91-B62A-6F66A3A7C8DD"));

            migrationBuilder.DropTable(
                name: "OfficeLocations");

            migrationBuilder.AddColumn<decimal>(
                name: "AllowedRadiusMeters",
                table: "Employees",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfficeLatitude",
                table: "Employees",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OfficeLongitude",
                table: "Employees",
                type: "decimal(9,6)",
                nullable: true);
        }
    }
}
