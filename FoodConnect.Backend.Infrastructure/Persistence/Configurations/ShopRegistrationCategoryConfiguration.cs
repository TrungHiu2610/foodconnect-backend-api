using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopRegistrationCategoryConfiguration : IEntityTypeConfiguration<ShopRegistrationCategory>
    {
        public void Configure(EntityTypeBuilder<ShopRegistrationCategory> builder)
        {
            builder.HasIndex(src => new { src.ShopRegistrationId, src.CategoryId })
                .IsUnique();

            builder.HasOne(src => src.ShopRegistration)
                .WithMany(sr => sr.ShopRegistrationCategories)
                .HasForeignKey(src => src.ShopRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(src => src.Category)
                .WithMany()
                .HasForeignKey(src => src.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
