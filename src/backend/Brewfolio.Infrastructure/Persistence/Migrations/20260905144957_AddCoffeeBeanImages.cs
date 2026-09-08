using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewfolio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260905144957_AddCoffeeBeanImages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImageKey",
            table: "CoffeeBeans",
            type: "nvarchar(320)",
            maxLength: 320,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ImageKey",
            table: "CoffeeBeans");
    }
}
