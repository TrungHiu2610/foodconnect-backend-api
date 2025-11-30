using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyUserShopRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Users_UserId",
                table: "Shops");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ShopId",
                table: "Users",
                column: "ShopId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Shops_ShopId",
                table: "Users",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Shops_ShopId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ShopId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Users_UserId",
                table: "Shops",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
