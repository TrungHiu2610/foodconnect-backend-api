using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class PromotionUsageConfiguration : IEntityTypeConfiguration<PromotionUsage>
    {
        public void Configure(EntityTypeBuilder<PromotionUsage> builder)
        {
            builder.Property(pu => pu.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(pu => pu.OrderValue)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(pu => pu.PromotionId);
            builder.HasIndex(pu => pu.UserId);
            builder.HasIndex(pu => pu.OrderId);
            builder.HasIndex(pu => new { pu.PromotionId, pu.UserId });

            builder.HasOne(pu => pu.Promotion)
                .WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pu => pu.Order)
                .WithMany()
                .HasForeignKey(pu => pu.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
