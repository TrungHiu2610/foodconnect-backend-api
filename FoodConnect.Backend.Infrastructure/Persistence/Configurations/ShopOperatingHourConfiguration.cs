using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopOperatingHoursConfiguration : IEntityTypeConfiguration<ShopOperatingHour>
    {
        public void Configure(EntityTypeBuilder<ShopOperatingHour> builder)
        {
            builder.HasKey(oh => oh.Id);

            builder.Property(oh => oh.DayOfWeek)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(oh => oh.OpenTime)
                .IsRequired();

            builder.Property(oh => oh.CloseTime)
                .IsRequired();

            builder.HasOne(oh => oh.ShopRegistration)
                .WithMany(sr => sr.OperatingHours)
                .HasForeignKey(oh => oh.ShopRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oh => oh.Shop)
                .WithMany(s => s.OperatingHours)
                .HasForeignKey(oh => oh.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
