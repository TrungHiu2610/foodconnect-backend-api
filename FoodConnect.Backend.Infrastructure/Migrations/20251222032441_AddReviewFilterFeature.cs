using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewFilterFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "ProductReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionDetails",
                table: "ProductReviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionReason",
                table: "ProductReviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProductReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "RejectionDetails",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ProductReviews");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProductReviews");
        }
    }
}
