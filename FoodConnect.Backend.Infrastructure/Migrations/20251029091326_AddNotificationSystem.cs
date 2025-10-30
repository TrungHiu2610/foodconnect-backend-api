using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPushed = table.Column<bool>(type: "boolean", nullable: false),
                    PushedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 9, 13, 24, 546, DateTimeKind.Utc).AddTicks(765));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 9, 13, 24, 546, DateTimeKind.Utc).AddTicks(792));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 9, 13, 24, 546, DateTimeKind.Utc).AddTicks(749));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 9, 13, 24, 546, DateTimeKind.Utc).AddTicks(797));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 9, 13, 24, 546, DateTimeKind.Utc).AddTicks(790));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OrderId",
                table: "Notifications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ShopId",
                table: "Notifications",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("2fa4cb76-edee-44e3-95e2-1f841b27929a"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 5, 1, 10, 313, DateTimeKind.Utc).AddTicks(1926));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("30d76555-7ff9-41d5-a11e-92a347f725bf"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 5, 1, 10, 313, DateTimeKind.Utc).AddTicks(1930));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 5, 1, 10, 313, DateTimeKind.Utc).AddTicks(1911));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 5, 1, 10, 313, DateTimeKind.Utc).AddTicks(1938));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 29, 5, 1, 10, 313, DateTimeKind.Utc).AddTicks(1928));
        }
    }
}
