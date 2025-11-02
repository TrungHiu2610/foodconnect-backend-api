using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopBuyerViewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPromoted",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOrderAt",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalOrders",
                table: "Shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 10, 26, 6, 598, DateTimeKind.Utc).AddTicks(3946));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 10, 26, 6, 598, DateTimeKind.Utc).AddTicks(3951));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 10, 26, 6, 598, DateTimeKind.Utc).AddTicks(3931));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 10, 26, 6, 598, DateTimeKind.Utc).AddTicks(3955));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 10, 26, 6, 598, DateTimeKind.Utc).AddTicks(3950));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "IsPromoted",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "LastOrderAt",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "TotalOrders",
                table: "Shops");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 9, 32, 40, 449, DateTimeKind.Utc).AddTicks(3414));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 9, 32, 40, 449, DateTimeKind.Utc).AddTicks(3419));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 9, 32, 40, 449, DateTimeKind.Utc).AddTicks(3395));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 9, 32, 40, 449, DateTimeKind.Utc).AddTicks(3424));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 11, 1, 9, 32, 40, 449, DateTimeKind.Utc).AddTicks(3417));
        }
    }
}
