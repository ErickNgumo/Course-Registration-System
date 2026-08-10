using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentCourseRegistration.Api.Infrastructure.Persistence;

#nullable disable

namespace StudentCourseRegistration.Api.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[DbContext(typeof(RegistrationDbContext))]
[Migration("20260804120000_AddAuditLogs")]
public partial class AddAuditLogs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AdministratorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Entity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Timestamp",
            table: "AuditLogs",
            column: "Timestamp");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Entity_EntityId",
            table: "AuditLogs",
            columns: new[] { "Entity", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_AdministratorId",
            table: "AuditLogs",
            column: "AdministratorId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs");
    }
}