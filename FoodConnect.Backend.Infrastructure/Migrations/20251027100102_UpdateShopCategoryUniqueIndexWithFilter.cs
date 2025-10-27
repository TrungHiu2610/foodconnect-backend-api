using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShopCategoryUniqueIndexWithFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories",
                columns: new[] { "ShopId", "CategoryId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories",
                columns: new[] { "ShopId", "CategoryId" },
                unique: true);
        }
    }
}
