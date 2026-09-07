using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brewfolio.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20260904121720_AddCoffeeBags : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CoffeeBags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CoffeeBeanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PurchasedOn = table.Column<DateOnly>(type: "date", nullable: false),
                RoastedOn = table.Column<DateOnly>(type: "date", nullable: true),
                OpenedOn = table.Column<DateOnly>(type: "date", nullable: true),
                InitialWeightGrams = table.Column<int>(type: "int", nullable: false),
                PricePaid = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                IsInStock = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CoffeeBags", x => x.Id);
                table.ForeignKey(
                    name: "FK_CoffeeBags_CoffeeBeans_CoffeeBeanId",
                    column: x => x.CoffeeBeanId,
                    principalTable: "CoffeeBeans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CoffeeBags_CoffeeBeanId_PurchasedOn",
            table: "CoffeeBags",
            columns: new[] { "CoffeeBeanId", "PurchasedOn" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CoffeeBags");
    }
}
