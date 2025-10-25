using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopCategoryConfiguration : IEntityTypeConfiguration<ShopCategory>
    {
        public void Configure(EntityTypeBuilder<ShopCategory> builder)
        {
            builder.HasKey(sc => sc.Id);

            builder.HasIndex(sc => new { sc.ShopId, sc.CategoryId })
                .IsUnique();

            builder.HasOne(sc => sc.Shop)
                .WithMany(s => s.ShopCategories)
                .HasForeignKey(sc => sc.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sc => sc.Category)
                .WithMany()
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
