using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteCascadeOnUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                value: new DateTime(2025, 10, 18, 12, 6, 45, 356, DateTimeKind.Utc).AddTicks(7157));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("04c7d90a-885f-44f3-b619-9c4ef3b5fd15"), new DateTime(2025, 10, 18, 12, 6, 45, 356, DateTimeKind.Utc).AddTicks(7175), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null },
                    { new Guid("0f000e9d-3c81-487c-b711-fdf3b1d4a40c"), new DateTime(2025, 10, 18, 12, 6, 45, 356, DateTimeKind.Utc).AddTicks(7215), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("7b87124e-cbc0-4092-a1d4-f2543e8144ca"), new DateTime(2025, 10, 18, 12, 6, 45, 356, DateTimeKind.Utc).AddTicks(7218), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null },
                    { new Guid("e95083c2-8553-4775-8122-df9019b69949"), new DateTime(2025, 10, 18, 12, 6, 45, 356, DateTimeKind.Utc).AddTicks(7213), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("04c7d90a-885f-44f3-b619-9c4ef3b5fd15"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("0f000e9d-3c81-487c-b711-fdf3b1d4a40c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7b87124e-cbc0-4092-a1d4-f2543e8144ca"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e95083c2-8553-4775-8122-df9019b69949"));

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
    }
}
