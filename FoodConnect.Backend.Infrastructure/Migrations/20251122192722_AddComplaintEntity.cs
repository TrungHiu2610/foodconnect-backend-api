using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderComplaintId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderComplaints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SellerResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsFixedAmount = table.Column<bool>(type: "boolean", nullable: false),
                    SellerDesiredRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AdminDecisionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedRefundAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComplaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderComplaints_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderComplaints_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderComplaints_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderComplaints_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderComplaintAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    OrderComplaintId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderComplaintAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderComplaintAssets_OrderComplaints_OrderComplaintId",
                        column: x => x.OrderComplaintId,
                        principalTable: "OrderComplaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplaintAssets_OrderComplaintId",
                table: "OrderComplaintAssets",
                column: "OrderComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplaints_AdminId",
                table: "OrderComplaints",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplaints_BuyerId",
                table: "OrderComplaints",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplaints_OrderId",
                table: "OrderComplaints",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderComplaints_SellerId",
                table: "OrderComplaints",
                column: "SellerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderComplaintAssets");

            migrationBuilder.DropTable(
                name: "OrderComplaints");

            migrationBuilder.DropColumn(
                name: "OrderComplaintId",
                table: "Orders");
        }
    }
}
