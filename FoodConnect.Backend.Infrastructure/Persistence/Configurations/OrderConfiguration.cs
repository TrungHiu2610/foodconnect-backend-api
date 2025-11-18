using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.PromotionId)
                .IsRequired(false);

            builder.Property(o => o.PromotionDiscountAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(o => o.PromotionCode)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.HasOne(o => o.Promotion)
                .WithMany()
                .HasForeignKey(o => o.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
