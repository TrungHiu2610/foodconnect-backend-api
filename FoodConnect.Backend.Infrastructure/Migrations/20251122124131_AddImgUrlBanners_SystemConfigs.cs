using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImgUrlBanners_SystemConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SystemConfigs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "SystemConfigs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SystemConfigs");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "SystemConfigs");
        }
    }
}
