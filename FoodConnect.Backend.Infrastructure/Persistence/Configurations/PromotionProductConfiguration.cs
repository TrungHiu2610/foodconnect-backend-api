using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
    {
        public void Configure(EntityTypeBuilder<PromotionProduct> builder)
        {
            builder.HasKey(pp => new { pp.PromotionId, pp.ProductId });

            builder.HasOne(pp => pp.Promotion)
                .WithMany(p => p.PromotionProducts)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pp => pp.Product)
                .WithMany()
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(pp => !pp.IsDeleted);
        }
    }
}
