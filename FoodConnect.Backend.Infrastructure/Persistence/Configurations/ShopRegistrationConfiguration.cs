using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopRegistrationConfiguration : IEntityTypeConfiguration<ShopRegistration>
    {
        public void Configure(EntityTypeBuilder<ShopRegistration> builder)
        {
            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.ShopName)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(sr => sr.ShopDescription)
                .IsRequired();

            builder.Property(sr => sr.PayoutMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(sr => sr.PayoutAccountInfo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sr => sr.PayoutAccountName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Street)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(sr => sr.Ward)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.District)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sr => sr.Latitude)
                .IsRequired();

            builder.Property(sr => sr.Longitude)
                .IsRequired();

            builder.HasOne(sr => sr.User)
                .WithMany() 
                .HasForeignKey(sr => sr.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(sr => sr.Assets)
                .WithOne(sra => sra.ShopRegistration)
                .HasForeignKey(sra => sra.SellerRegistrationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(sr => sr.OperatingHours)
                .WithOne(oh => oh.ShopRegistration)
                .HasForeignKey(oh => oh.ShopRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(sr => sr.ShopRegistrationCategories)
                .WithOne(src => src.ShopRegistration)
                .HasForeignKey(src => src.ShopRegistrationId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
