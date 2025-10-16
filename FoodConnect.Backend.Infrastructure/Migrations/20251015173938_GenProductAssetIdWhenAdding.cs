using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GenProductAssetIdWhenAdding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2d3c171b-0eff-469c-9fea-0250b2689414"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("6de54913-ac70-43ab-a3fd-9b28add0e873"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("89a67c58-8919-4451-986f-9a8d271e3bcd"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d3bec952-40b8-452d-a23e-d54a30a47f81"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 15, 17, 39, 37, 606, DateTimeKind.Utc).AddTicks(5498));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("3549102f-63fb-49cc-85f4-574a94cf8331"), new DateTime(2025, 10, 15, 17, 39, 37, 606, DateTimeKind.Utc).AddTicks(5566), null, "Dairy products", "https://example.com/images/dairy.jpg", true, "Dairy", null, null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("5b15cb6b-bdb0-4642-98d7-518e05c4d6de"), new DateTime(2025, 10, 15, 17, 39, 37, 606, DateTimeKind.Utc).AddTicks(5568), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("a056116f-a698-44c5-8c10-cfac1e8d72b1"), new DateTime(2025, 10, 15, 17, 39, 37, 606, DateTimeKind.Utc).AddTicks(5581), null, "Fresh berries", "https://example.com/images/berries.jpg", true, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("b870c168-1a46-42f0-8fc6-ad0c0176839d"), new DateTime(2025, 10, 15, 17, 39, 37, 606, DateTimeKind.Utc).AddTicks(5563), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, "Vegetables", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3549102f-63fb-49cc-85f4-574a94cf8331"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5b15cb6b-bdb0-4642-98d7-518e05c4d6de"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a056116f-a698-44c5-8c10-cfac1e8d72b1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b870c168-1a46-42f0-8fc6-ad0c0176839d"));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 10, 5, 41, 18, 51, DateTimeKind.Utc).AddTicks(9246));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("2d3c171b-0eff-469c-9fea-0250b2689414"), new DateTime(2025, 10, 10, 5, 41, 18, 51, DateTimeKind.Utc).AddTicks(9262), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, "Vegetables", null, null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("6de54913-ac70-43ab-a3fd-9b28add0e873"), new DateTime(2025, 10, 10, 5, 41, 18, 51, DateTimeKind.Utc).AddTicks(9264), null, "Dairy products", "https://example.com/images/dairy.jpg", true, "Dairy", null, null, null },
                    { new Guid("89a67c58-8919-4451-986f-9a8d271e3bcd"), new DateTime(2025, 10, 10, 5, 41, 18, 51, DateTimeKind.Utc).AddTicks(9279), null, "Fresh berries", "https://example.com/images/berries.jpg", true, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("d3bec952-40b8-452d-a23e-d54a30a47f81"), new DateTime(2025, 10, 10, 5, 41, 18, 51, DateTimeKind.Utc).AddTicks(9275), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });
        }
    }
}
