using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSellerWalletTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_SellerWalletId",
                table: "WithdrawalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_Users_SellerId",
                table: "WithdrawalRequests");

            migrationBuilder.DropTable(
                name: "SellerWalletTransactions");

            migrationBuilder.DropTable(
                name: "SellerWallets");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_SellerId",
                table: "WithdrawalRequests");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_SellerWalletId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "SellerWalletId",
                table: "WithdrawalRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "WithdrawalRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SellerWalletId",
                table: "WithdrawalRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SellerWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    PendingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalEarned = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerWallets_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SellerWalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    WithdrawalRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerWalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerWalletTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SellerWalletTransactions_SellerWallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "SellerWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SellerWalletTransactions_WithdrawalRequests_WithdrawalReque~",
                        column: x => x.WithdrawalRequestId,
                        principalTable: "WithdrawalRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_SellerId",
                table: "WithdrawalRequests",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_SellerWalletId",
                table: "WithdrawalRequests",
                column: "SellerWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerWallets_SellerId",
                table: "SellerWallets",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerWalletTransactions_OrderId",
                table: "SellerWalletTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerWalletTransactions_WalletId",
                table: "SellerWalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerWalletTransactions_WithdrawalRequestId",
                table: "SellerWalletTransactions",
                column: "WithdrawalRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_SellerWalletId",
                table: "WithdrawalRequests",
                column: "SellerWalletId",
                principalTable: "SellerWallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_Users_SellerId",
                table: "WithdrawalRequests",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
