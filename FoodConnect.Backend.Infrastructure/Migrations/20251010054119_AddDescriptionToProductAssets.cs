using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToProductAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1a8a8b72-3bf2-490b-94a8-c7120bfb361c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7645e6be-3909-4672-a476-ec43123cac3e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("86345f9b-9a30-40e2-b367-86b1290007f7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c88e2e05-0d6c-4be4-94da-c9a112bc088c"));

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                table: "ProductAssets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssetDescription",
                table: "ProductAssets",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "AssetDescription",
                table: "ProductAssets");

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                table: "ProductAssets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 9, 4, 54, 5, 690, DateTimeKind.Utc).AddTicks(7297));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1a8a8b72-3bf2-490b-94a8-c7120bfb361c"), new DateTime(2025, 10, 9, 4, 54, 5, 690, DateTimeKind.Utc).AddTicks(7312), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, "Vegetables", null, null, null },
                    { new Guid("7645e6be-3909-4672-a476-ec43123cac3e"), new DateTime(2025, 10, 9, 4, 54, 5, 690, DateTimeKind.Utc).AddTicks(7316), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("86345f9b-9a30-40e2-b367-86b1290007f7"), new DateTime(2025, 10, 9, 4, 54, 5, 690, DateTimeKind.Utc).AddTicks(7328), null, "Fresh berries", "https://example.com/images/berries.jpg", true, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null },
                    { new Guid("c88e2e05-0d6c-4be4-94da-c9a112bc088c"), new DateTime(2025, 10, 9, 4, 54, 5, 690, DateTimeKind.Utc).AddTicks(7314), null, "Dairy products", "https://example.com/images/dairy.jpg", true, "Dairy", null, null, null }
                });
        }
    }
}
