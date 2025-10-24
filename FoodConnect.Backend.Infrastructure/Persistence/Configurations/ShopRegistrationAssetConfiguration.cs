using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopRegistrationAssetConfiguration : IEntityTypeConfiguration<ShopRegistrationAsset>
    {
        public void Configure(EntityTypeBuilder<ShopRegistrationAsset> builder)
        {
            builder.HasKey(sra => sra.Id);

            builder.Property(sra => sra.AssetUrl)
                .IsRequired();

            builder.Property(sra => sra.AssetType)
                .HasConversion<int>()
                .IsRequired();
        }
    }
}
