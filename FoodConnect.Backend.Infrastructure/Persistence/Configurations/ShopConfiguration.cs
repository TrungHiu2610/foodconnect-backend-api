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

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            builder.Property(s => s.CoverImageUrl)
                .HasMaxLength(2048);

            builder.Property(s => s.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.Rating)
                .HasColumnType("decimal(3,2)");

            builder.HasOne(s => s.User)
                .WithOne() 
                .HasForeignKey<Shop>(s => s.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.OperatingHours)
                .WithOne(oh => oh.Shop)
                .HasForeignKey(oh => oh.ShopId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Products)
                .WithOne(p => p.Shop)
                .HasForeignKey(p => p.ShopId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
