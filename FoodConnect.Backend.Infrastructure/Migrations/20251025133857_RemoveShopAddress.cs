using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShopAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2feb568e-736e-40fe-9bb1-5c5ef23c83d8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4c98edea-da6e-40dd-b86a-05317180db73"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b146ef79-6089-475a-8f3f-b453b39f6033"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d0800510-87b5-425e-b357-ee6f2133a402"));

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Shops");

            migrationBuilder.AlterColumn<string>(
                name: "Ward",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Shops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Shops",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Shops",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 25, 13, 38, 55, 668, DateTimeKind.Utc).AddTicks(6508));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1f107c3d-468f-4c25-b256-76feabefc788"), new DateTime(2025, 10, 25, 13, 38, 55, 668, DateTimeKind.Utc).AddTicks(6526), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null },
                    { new Guid("ba060b89-e36a-42d4-99d6-004d0fb8dd89"), new DateTime(2025, 10, 25, 13, 38, 55, 668, DateTimeKind.Utc).AddTicks(6587), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("d4f8181f-73ec-4bad-a60b-676ad680205f"), new DateTime(2025, 10, 25, 13, 38, 55, 668, DateTimeKind.Utc).AddTicks(6584), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null },
                    { new Guid("ef2ec044-4dac-47fb-8a66-b068f150e463"), new DateTime(2025, 10, 25, 13, 38, 55, 668, DateTimeKind.Utc).AddTicks(6590), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("1f107c3d-468f-4c25-b256-76feabefc788"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ba060b89-e36a-42d4-99d6-004d0fb8dd89"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d4f8181f-73ec-4bad-a60b-676ad680205f"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ef2ec044-4dac-47fb-8a66-b068f150e463"));

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ShopName",
                table: "Shops");

            migrationBuilder.AlterColumn<string>(
                name: "Ward",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "Shops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Shops",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Shops",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "District",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Shops",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 25, 5, 8, 44, 790, DateTimeKind.Utc).AddTicks(5850));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("2feb568e-736e-40fe-9bb1-5c5ef23c83d8"), new DateTime(2025, 10, 25, 5, 8, 44, 790, DateTimeKind.Utc).AddTicks(5914), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null },
                    { new Guid("4c98edea-da6e-40dd-b86a-05317180db73"), new DateTime(2025, 10, 25, 5, 8, 44, 790, DateTimeKind.Utc).AddTicks(5907), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("b146ef79-6089-475a-8f3f-b453b39f6033"), new DateTime(2025, 10, 25, 5, 8, 44, 790, DateTimeKind.Utc).AddTicks(5865), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null },
                    { new Guid("d0800510-87b5-425e-b357-ee6f2133a402"), new DateTime(2025, 10, 25, 5, 8, 44, 790, DateTimeKind.Utc).AddTicks(5909), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });
        }
    }
}
