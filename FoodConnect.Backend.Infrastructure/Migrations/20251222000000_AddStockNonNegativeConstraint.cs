using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockNonNegativeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CHECK constraint to prevent negative stock
            // This is a safety net in addition to pessimistic locking
            migrationBuilder.Sql(@"
                ALTER TABLE ""Products"" 
                ADD CONSTRAINT ""CK_Products_StockQuantity_NonNegative"" 
                CHECK (""StockQuantity"" IS NULL OR ""StockQuantity"" >= 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Products"" 
                DROP CONSTRAINT IF EXISTS ""CK_Products_StockQuantity_NonNegative"";
            ");
        }
    }
}
