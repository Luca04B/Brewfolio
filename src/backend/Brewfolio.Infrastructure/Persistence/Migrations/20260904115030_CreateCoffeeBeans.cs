using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewfolio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260904115030_CreateCoffeeBeans : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CoffeeBeans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Roaster = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CoffeeBeans", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CoffeeBeans_CreatedAt",
            table: "CoffeeBeans",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_CoffeeBeans_Roaster_Name",
            table: "CoffeeBeans",
            columns: new[] { "Roaster", "Name" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CoffeeBeans");
    }
}
