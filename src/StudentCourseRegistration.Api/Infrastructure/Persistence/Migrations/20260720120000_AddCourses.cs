using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentCourseRegistration.Api.Infrastructure.Persistence;

#nullable disable

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RegistrationDbContext))]
[Migration("20260720120000_AddCourses")]
public partial class AddCourses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                Credits = table.Column<int>(type: "int", nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                Semester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.Id);
                table.CheckConstraint("CK_Courses_Capacity_NonNegative", "[Capacity] >= 0");
                table.CheckConstraint("CK_Courses_Credits_Positive", "[Credits] > 0");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Courses_Code",
            table: "Courses",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Courses_IsActive_Semester",
            table: "Courses",
            columns: new[] { "IsActive", "Semester" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Courses");
    }
}
