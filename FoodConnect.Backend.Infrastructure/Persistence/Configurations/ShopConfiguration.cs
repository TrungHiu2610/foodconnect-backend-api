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
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.UserId)
                .IsUnique();

            builder.Property(s => s.ShopName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.LogoUrl)
                .HasMaxLength(2048);

            builder.Property(s => s.CoverImageUrl)
                .HasMaxLength(2048);

            builder.Property(s => s.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.Rating)
                .HasColumnType("decimal(3,2)");

            builder.Property(s => s.AdminReason)
                .HasMaxLength(500);

            builder.Property(s => s.PayoutMethod)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.PayoutAccountInfo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.PayoutAccountName)
                .IsRequired()
                .HasMaxLength(100);

            // Optional detailed address fields
            builder.Property(s => s.Street)
                .HasMaxLength(255);

            builder.Property(s => s.Ward)
                .HasMaxLength(100);

            builder.Property(s => s.District)
                .HasMaxLength(100);

            builder.Property(s => s.City)
                .HasMaxLength(100);

            builder.Property(s => s.Country)
                .HasMaxLength(100);

            // Ignore calculated fields (not mapped to database)
            builder.Ignore(s => s.CalculatedDistance);

            // 1-1 Relationship: User (Principal) ← Shop (Dependent)
            // Shop.UserId is the foreign key
            builder.HasOne(s => s.User)
                .WithOne(u => u.Shop)  // Bidirectional navigation
                .HasForeignKey<Shop>(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.OperatingHours)
                .WithOne(oh => oh.Shop)
                .HasForeignKey(oh => oh.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Products)
                .WithOne(p => p.Shop)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Assets)
                .WithOne(a => a.Shop)
                .HasForeignKey(a => a.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.ShopCategories)
                .WithOne(sc => sc.Shop)
                .HasForeignKey(sc => sc.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
