using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupAndImplementHardDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop old index with filter
            migrationBuilder.DropIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories");

            // Step 2: Cleanup soft-deleted records from junction tables before implementing hard delete
            // This removes all records where IsDeleted = true
            migrationBuilder.Sql(@"
                DELETE FROM ""ShopCategories"" WHERE ""IsDeleted"" = true;
                DELETE FROM ""ShopOperatingHours"" WHERE ""IsDeleted"" = true;
            ");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 36, 20, 453, DateTimeKind.Utc).AddTicks(9527));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 36, 20, 453, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 36, 20, 453, DateTimeKind.Utc).AddTicks(9510));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 36, 20, 453, DateTimeKind.Utc).AddTicks(9539));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 36, 20, 453, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.CreateIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories",
                columns: new[] { "ShopId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 1, 1, 720, DateTimeKind.Utc).AddTicks(9359));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 1, 1, 720, DateTimeKind.Utc).AddTicks(9366));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 1, 1, 720, DateTimeKind.Utc).AddTicks(9343));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 1, 1, 720, DateTimeKind.Utc).AddTicks(9369));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 10, 1, 1, 720, DateTimeKind.Utc).AddTicks(9363));

            migrationBuilder.CreateIndex(
                name: "IX_ShopCategories_ShopId_CategoryId",
                table: "ShopCategories",
                columns: new[] { "ShopId", "CategoryId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
