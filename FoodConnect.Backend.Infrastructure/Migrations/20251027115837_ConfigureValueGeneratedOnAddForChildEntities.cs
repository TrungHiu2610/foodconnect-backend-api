using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureValueGeneratedOnAddForChildEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 58, 37, 498, DateTimeKind.Utc).AddTicks(5261));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 58, 37, 498, DateTimeKind.Utc).AddTicks(5289));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 58, 37, 498, DateTimeKind.Utc).AddTicks(5240));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 58, 37, 498, DateTimeKind.Utc).AddTicks(5293));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 58, 37, 498, DateTimeKind.Utc).AddTicks(5263));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 38, 43, 64, DateTimeKind.Utc).AddTicks(5418));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 38, 43, 64, DateTimeKind.Utc).AddTicks(5422));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 38, 43, 64, DateTimeKind.Utc).AddTicks(5403));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 38, 43, 64, DateTimeKind.Utc).AddTicks(5425));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 27, 11, 38, 43, 64, DateTimeKind.Utc).AddTicks(5420));
        }
    }
}
