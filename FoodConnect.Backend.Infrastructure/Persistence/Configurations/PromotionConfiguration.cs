using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.Property(p => p.PromotionName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.DiscountValue)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.MinimumOrderValue)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(p => p.UsagePerCustomer)
                .HasDefaultValue(1);

            builder.Property(p => p.TotalUsedCount)
                .HasDefaultValue(0);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.PromotionType)
                .IsRequired();

            builder.Property(p => p.AdminReason)
                .HasMaxLength(1000);

            builder.HasIndex(p => p.ShopId);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => new { p.StartDate, p.EndDate });

            builder.HasOne(p => p.Shop)
                .WithMany()
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.PromotionProducts)
                .WithOne(pp => pp.Promotion)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
