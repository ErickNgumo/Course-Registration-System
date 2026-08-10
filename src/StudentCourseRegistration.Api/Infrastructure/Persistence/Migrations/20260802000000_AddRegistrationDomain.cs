using System;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentCourseRegistration.Api.Infrastructure.Persistence;

#nullable disable

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRegistrationDomain : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                RegisteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                DroppedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                FinalGrade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => x.Id);
                table.CheckConstraint("CK_Enrollments_DroppedAt_AfterRegisteredAt", "[DroppedAt] IS NULL OR [DroppedAt] >= [RegisteredAt]");
                table.ForeignKey(
                    name: "FK_Enrollments_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Enrollments_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CourseSchedules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DayOfWeek = table.Column<int>(type: "int", nullable: false),
                StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CourseSchedules", x => x.Id);
                table.CheckConstraint("CK_CourseSchedules_EndTime_AfterStartTime", "[EndTime] > [StartTime]");
                table.ForeignKey(
                    name: "FK_CourseSchedules_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CoursePrerequisites",
            columns: table => new
            {
                CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PrerequisiteCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CoursePrerequisites", x => new { x.CourseId, x.PrerequisiteCourseId });
                table.CheckConstraint("CK_CoursePrerequisites_Prerequisite_Differs", "[CourseId] <> [PrerequisiteCourseId]");
                table.ForeignKey(
                    name: "FK_CoursePrerequisites_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CoursePrerequisites_Courses_PrerequisiteCourseId",
                    column: x => x.PrerequisiteCourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_StudentId_CourseId",
            table: "Enrollments",
            columns: new[] { "StudentId", "CourseId" });

        migrationBuilder.CreateIndex(
            name: "IX_Enrollments_StudentId_CourseId_Status",
            table: "Enrollments",
            columns: new[] { "StudentId", "CourseId", "Status" },
            filter: "[Status] IN ('Registered', 'Waitlisted')");

        migrationBuilder.CreateIndex(
            name: "IX_CourseSchedules_CourseId_DayOfWeek_StartTime_EndTime",
            table: "CourseSchedules",
            columns: new[] { "CourseId", "DayOfWeek", "StartTime", "EndTime" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CoursePrerequisites");
        migrationBuilder.DropTable(name: "CourseSchedules");
        migrationBuilder.DropTable(name: "Enrollments");
    }
}
