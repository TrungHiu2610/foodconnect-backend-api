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
    public class ProductDailyAvailabilityConfiguration : IEntityTypeConfiguration<ProductDailyAvailability>
    {
        public void Configure(EntityTypeBuilder<ProductDailyAvailability> builder)
        {
            builder.HasKey(pda => pda.Id);
            builder.Property(pda => pda.Date).IsRequired();
            builder.Property(pda => pda.Quantity).IsRequired();
            builder.HasOne(pda => pda.Product)
                   .WithMany(p => p.ProductDailyAvailabilities)
                   .HasForeignKey(pda => pda.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
