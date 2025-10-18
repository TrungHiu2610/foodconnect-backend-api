using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleEnumConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("146620aa-7449-46e1-b5d5-5c05cd473e75"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4f8b63ca-f87d-4e3b-b98a-2f36e56280c6"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("629d5413-853b-42a0-bd1c-626862a9ea95"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("79572ac5-129b-4c8b-867b-8a2e2eb1e498"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 18, 12, 2, 47, 563, DateTimeKind.Utc).AddTicks(4411));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("053ac5ba-e241-4504-a7d8-d50f4ae3e71e"), new DateTime(2025, 10, 18, 12, 2, 47, 563, DateTimeKind.Utc).AddTicks(4463), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("16c9d523-f76c-4dc9-bcfa-069b25ecbc8d"), new DateTime(2025, 10, 18, 12, 2, 47, 563, DateTimeKind.Utc).AddTicks(4465), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("b9fa1aa4-d27a-4701-82f0-c607b8584715"), new DateTime(2025, 10, 18, 12, 2, 47, 563, DateTimeKind.Utc).AddTicks(4469), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("d0fc1e15-1e2a-480f-b250-ad815c918e1c"), new DateTime(2025, 10, 18, 12, 2, 47, 563, DateTimeKind.Utc).AddTicks(4430), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("053ac5ba-e241-4504-a7d8-d50f4ae3e71e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("16c9d523-f76c-4dc9-bcfa-069b25ecbc8d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b9fa1aa4-d27a-4701-82f0-c607b8584715"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d0fc1e15-1e2a-480f-b250-ad815c918e1c"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8470));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("146620aa-7449-46e1-b5d5-5c05cd473e75"), new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8503), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("4f8b63ca-f87d-4e3b-b98a-2f36e56280c6"), new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8485), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null },
                    { new Guid("629d5413-853b-42a0-bd1c-626862a9ea95"), new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8490), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("79572ac5-129b-4c8b-867b-8a2e2eb1e498"), new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8488), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null });
        }
    }
}
