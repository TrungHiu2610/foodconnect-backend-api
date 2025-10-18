using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductDailyAvailabilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductAssets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                columns: new[] { "CreatedAtUtc", "IsDeleted" },
                values: new object[] { new DateTime(2025, 10, 16, 13, 20, 49, 71, DateTimeKind.Utc).AddTicks(8470), false });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductDailyAvailabilities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductAssets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

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
    }
}
