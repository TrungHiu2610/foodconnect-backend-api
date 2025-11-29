using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComplaintFeaturePhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsFixedAmount",
                table: "OrderComplaints",
                newName: "IsSellerRefundFixedAmount");

            migrationBuilder.AddColumn<DateTime>(
                name: "AdminDecidedAt",
                table: "OrderComplaints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "OrderComplaints",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "OrderComplaints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SellerDesiredRefundPercentage",
                table: "OrderComplaints",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SellerRespondedAt",
                table: "OrderComplaints",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminDecidedAt",
                table: "OrderComplaints");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "OrderComplaints");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "OrderComplaints");

            migrationBuilder.DropColumn(
                name: "SellerDesiredRefundPercentage",
                table: "OrderComplaints");

            migrationBuilder.DropColumn(
                name: "SellerRespondedAt",
                table: "OrderComplaints");

            migrationBuilder.RenameColumn(
                name: "IsSellerRefundFixedAmount",
                table: "OrderComplaints",
                newName: "IsFixedAmount");
        }
    }
}
