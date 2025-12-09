using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Play.Trading.Service.Data.Migrations.Catalog
{
    /// <inheritdoc />
    public partial class CreateInitialCatalogItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CatalogItems",
                columns: new[] { "Id", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("7e73026b-52a9-4cd8-9b8f-6a24a6b504f2"), "Cures poisen", "Antidote", 8m },
                    { new Guid("802f84f0-8e91-41fd-b72d-46f8ca40bb2e"), "Deals a small amount of damage", "Bronze sword", 10m },
                    { new Guid("92453fe9-daba-47f8-8e81-257a7dd1aea7"), "Restores a small amount of HP", "Potion", 6m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems");
        }
    }
}
