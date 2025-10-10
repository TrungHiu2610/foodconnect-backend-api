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
    public class ProductAssetConfiguration : IEntityTypeConfiguration<ProductAsset>
    {
        public void Configure(EntityTypeBuilder<ProductAsset> builder)
        {
            builder.HasKey(pa => pa.Id);
            builder.Property(pa => pa.AssetName).HasMaxLength(255);
            builder.Property(pa => pa.AssetDescription).HasMaxLength(1000);
            builder.Property(pa => pa.AssetUrl).IsRequired().HasMaxLength(2048);
            builder.Property(pa => pa.AssetType).IsRequired();
            builder.Property(pa => pa.IsThumbnail).IsRequired();
            builder.HasOne(pa => pa.Product)
                   .WithMany(p => p.ProductAssets)
                   .HasForeignKey(pa => pa.ProductId);
        }
    }
}
