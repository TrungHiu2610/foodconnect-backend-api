using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerInfoToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SellerEmail",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerFullName",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerPhone",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 12, 4, 33, 643, DateTimeKind.Utc).AddTicks(8475));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 12, 4, 33, 643, DateTimeKind.Utc).AddTicks(8480));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 12, 4, 33, 643, DateTimeKind.Utc).AddTicks(8460));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 12, 4, 33, 643, DateTimeKind.Utc).AddTicks(8484));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 12, 4, 33, 643, DateTimeKind.Utc).AddTicks(8478));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerEmail",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SellerFullName",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "SellerPhone",
                table: "Shops");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 28, 13, 11, 40, 363, DateTimeKind.Utc).AddTicks(9862));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 28, 13, 11, 40, 363, DateTimeKind.Utc).AddTicks(9867));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 28, 13, 11, 40, 363, DateTimeKind.Utc).AddTicks(9847));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 28, 13, 11, 40, 363, DateTimeKind.Utc).AddTicks(9871));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 28, 13, 11, 40, 363, DateTimeKind.Utc).AddTicks(9865));
        }
    }
}
