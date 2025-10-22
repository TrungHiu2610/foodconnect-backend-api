using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerRegistrationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Users_UserId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_UserId",
                table: "Shops");

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

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Shops",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Shops",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Shops",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ward",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ShopOperatingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopOperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopOperatingHours_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShopDescription = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdminReason = table.Column<string>(type: "text", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopRegistrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopRegistrationAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetUrl = table.Column<string>(type: "text", nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    SellerRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRegistrationAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopRegistrationAssets_ShopRegistrations_SellerRegistration~",
                        column: x => x.SellerRegistrationId,
                        principalTable: "ShopRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("4286ba6a-3d40-46be-8539-237190c067b6"),
                column: "CreatedAtUtc",
                value: new DateTime(2025, 10, 22, 15, 3, 41, 507, DateTimeKind.Utc).AddTicks(8602));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("420cf66a-28d4-4cab-af92-35612e6f3174"), new DateTime(2025, 10, 22, 15, 3, 41, 507, DateTimeKind.Utc).AddTicks(8654), null, "Dairy products", "https://example.com/images/dairy.jpg", true, false, "Dairy", null, null, null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "DeliveryType", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("5dcd9809-5722-4535-bf61-8404ae374ef9"), new DateTime(2025, 10, 22, 15, 3, 41, 507, DateTimeKind.Utc).AddTicks(8621), null, 1, "Fresh vegetables", "https://example.com/images/vegetables.jpg", true, false, "Vegetables", null, null, null },
                    { new Guid("9bcb08a1-4db0-4460-b2e3-bf48095b2d90"), new DateTime(2025, 10, 22, 15, 3, 41, 507, DateTimeKind.Utc).AddTicks(8656), null, 1, "Fresh citrus fruits", "https://example.com/images/citrus.jpg", true, false, "Citrus Fruits", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Description", "ImageUrl", "IsActive", "IsDeleted", "Name", "ParentId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { new Guid("a31e92ec-0390-4921-b419-ada591f01957"), new DateTime(2025, 10, 22, 15, 3, 41, 507, DateTimeKind.Utc).AddTicks(8659), null, "Fresh berries", "https://example.com/images/berries.jpg", true, false, "Berries", new Guid("4286ba6a-3d40-46be-8539-237190c067b6"), null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_UserId",
                table: "Shops",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopOperatingHours_ShopId",
                table: "ShopOperatingHours",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopRegistrationAssets_SellerRegistrationId",
                table: "ShopRegistrationAssets",
                column: "SellerRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopRegistrations_UserId",
                table: "ShopRegistrations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Users_UserId",
                table: "Shops",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Users_UserId",
                table: "Shops");

            migrationBuilder.DropTable(
                name: "ShopOperatingHours");

            migrationBuilder.DropTable(
                name: "ShopRegistrationAssets");

            migrationBuilder.DropTable(
                name: "ShopRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Shops_UserId",
                table: "Shops");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("420cf66a-28d4-4cab-af92-35612e6f3174"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("5dcd9809-5722-4535-bf61-8404ae374ef9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("9bcb08a1-4db0-4460-b2e3-bf48095b2d90"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a31e92ec-0390-4921-b419-ada591f01957"));

            migrationBuilder.DropColumn(
                name: "City",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Ward",
                table: "Shops");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Shops",
                newName: "userId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Shops",
                type: "text",
                nullable: false,
                defaultValue: "PendingApproval",
                oldClrType: typeof(int),
                oldType: "integer");

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

            migrationBuilder.CreateIndex(
                name: "IX_Shops_userId",
                table: "Shops",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Users_userId",
                table: "Shops",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
