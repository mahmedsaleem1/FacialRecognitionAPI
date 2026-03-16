using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacialRecognitionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeOfficeLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
