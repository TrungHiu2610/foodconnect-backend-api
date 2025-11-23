using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedWalletSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop existing foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_WalletId",
                table: "WithdrawalRequests");

            // Step 2: Rename existing WalletId column to SellerWalletId
            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "WithdrawalRequests",
                newName: "SellerWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_WithdrawalRequests_WalletId",
                table: "WithdrawalRequests",
                newName: "IX_WithdrawalRequests_SellerWalletId");

            // Step 3: Add new nullable WalletId column for new Wallet system
            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "WithdrawalRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletType = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalEarned = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "numeric", nullable: false),
                    PendingBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalSpent = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    WithdrawalRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_WithdrawalRequests_WithdrawalRequestId",
                        column: x => x.WithdrawalRequestId,
                        principalTable: "WithdrawalRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_WalletId",
                table: "WithdrawalRequests",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_OrderId",
                table: "WalletTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WithdrawalRequestId",
                table: "WalletTransactions",
                column: "WithdrawalRequestId");

            // Step 4: Re-add foreign key for SellerWalletId (old system)
            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_SellerWalletId",
                table: "WithdrawalRequests",
                column: "SellerWalletId",
                principalTable: "SellerWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Step 5: Add foreign key for new WalletId (new system) - nullable, no cascade
            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_Wallets_WalletId",
                table: "WithdrawalRequests",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse Step 5: Drop new foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_SellerWalletId",
                table: "WithdrawalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawalRequests_Wallets_WalletId",
                table: "WithdrawalRequests");

            // Reverse Step 4: Drop new tables
            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Wallets");

            // Reverse Step 3: Drop new WalletId column
            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_WalletId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "WithdrawalRequests");

            // Reverse Step 2: Rename SellerWalletId back to WalletId
            migrationBuilder.RenameColumn(
                name: "SellerWalletId",
                table: "WithdrawalRequests",
                newName: "WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_WithdrawalRequests_SellerWalletId",
                table: "WithdrawalRequests",
                newName: "IX_WithdrawalRequests_WalletId");

            // Reverse Step 1: Re-add original foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawalRequests_SellerWallets_WalletId",
                table: "WithdrawalRequests",
                column: "WalletId",
                principalTable: "SellerWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
