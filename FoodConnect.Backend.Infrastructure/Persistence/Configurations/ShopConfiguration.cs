using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopConfiguration : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(s => s.Description)
                .HasMaxLength(500);
            builder.Property(s => s.CoverImageUrl)
                .HasMaxLength(2048);
            builder.Property(s => s.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (ShopStatusEnum)Enum.Parse(typeof(ShopStatusEnum), v))
                .IsRequired()
                .HasDefaultValue(ShopStatusEnum.PendingApproval);
            builder.Property(s => s.Rating)
                .HasColumnType("decimal(3,2)");
            builder.HasMany(s => s.Products)
                .WithOne(p => p.Shop)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
