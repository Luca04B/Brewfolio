using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewfolio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260904120902_CompleteCoffeeBeanProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "CoffeeBeans",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Origin",
            table: "CoffeeBeans",
            type: "nvarchar(240)",
            maxLength: 240,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProductUrl",
            table: "CoffeeBeans",
            type: "nvarchar(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RoastLevel",
            table: "CoffeeBeans",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "CoffeeBeans");

        migrationBuilder.DropColumn(
            name: "Origin",
            table: "CoffeeBeans");

        migrationBuilder.DropColumn(
            name: "ProductUrl",
            table: "CoffeeBeans");

        migrationBuilder.DropColumn(
            name: "RoastLevel",
            table: "CoffeeBeans");
    }
}
