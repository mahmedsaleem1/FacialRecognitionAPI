using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FacialRecognitionAPI.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEmployeeAndAttendanceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendance_EmployeeId_MarkedAt",
                table: "Attendance");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "AttendanceDate",
                table: "Attendance",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceStatusId",
                table: "Attendance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AttendanceStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "present" },
                    { 2, "absent" },
                    { 3, "late" }
                });

            migrationBuilder.Sql(@"
                INSERT INTO Departments (Name)
                SELECT DISTINCT Department
                FROM Employees
                WHERE Department IS NOT NULL AND LTRIM(RTRIM(Department)) <> ''
            ");

            migrationBuilder.Sql(@"
                UPDATE e
                SET DepartmentId = d.Id
                FROM Employees e
                INNER JOIN Departments d ON d.Name = e.Department
            ");

            migrationBuilder.Sql(@"
                INSERT INTO Positions (Name)
                SELECT DISTINCT Position
                FROM Employees
                WHERE Position IS NOT NULL AND LTRIM(RTRIM(Position)) <> ''
            ");

            migrationBuilder.Sql(@"
                UPDATE e
                SET PositionId = p.Id
                FROM Employees e
                INNER JOIN Positions p ON p.Name = e.Position
            ");

            migrationBuilder.Sql(@"
                UPDATE Attendance
                SET AttendanceDate = CAST(MarkedAt AS date)
                WHERE AttendanceDate IS NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE a
                SET AttendanceStatusId = s.Id
                FROM Attendance a
                INNER JOIN AttendanceStatuses s ON s.Name = LOWER(a.Status)
                WHERE a.Status IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE Attendance
                SET AttendanceStatusId = 1
                WHERE AttendanceStatusId IS NULL
            ");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "AttendanceDate",
                table: "Attendance",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AttendanceStatusId",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Attendance");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PositionId",
                table: "Employees",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_AttendanceDate",
                table: "Attendance",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_AttendanceStatusId",
                table: "Attendance",
                column: "AttendanceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EmployeeId_AttendanceDate",
                table: "Attendance",
                columns: new[] { "EmployeeId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatuses_Name",
                table: "AttendanceStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Name",
                table: "Positions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_AttendanceStatuses_AttendanceStatusId",
                table: "Attendance",
                column: "AttendanceStatusId",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_AttendanceStatuses_AttendanceStatusId",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Positions_PositionId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "AttendanceStatuses");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PositionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_AttendanceDate",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_AttendanceStatusId",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_EmployeeId_AttendanceDate",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AttendanceDate",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "AttendanceStatusId",
                table: "Attendance");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Attendance",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "present");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EmployeeId_MarkedAt",
                table: "Attendance",
                columns: new[] { "EmployeeId", "MarkedAt" },
                unique: true);
        }
    }
}
