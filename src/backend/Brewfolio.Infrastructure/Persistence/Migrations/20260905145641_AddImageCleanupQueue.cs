using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewfolio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260905145641_AddImageCleanupQueue : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ImageCleanupJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ImageKey = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                AttemptCount = table.Column<int>(type: "int", nullable: false),
                NextAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImageCleanupJobs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ImageCleanupJobs_NextAttemptAt",
            table: "ImageCleanupJobs",
            column: "NextAttemptAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ImageCleanupJobs");
    }
}
