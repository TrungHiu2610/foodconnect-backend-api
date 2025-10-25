using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class ShopAssetConfiguration : IEntityTypeConfiguration<ShopAsset>
    {
        public void Configure(EntityTypeBuilder<ShopAsset> builder)
        {
            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.AssetUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(sa => sa.AssetType)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(sa => sa.Shop)
                .WithMany(s => s.Assets)
                .HasForeignKey(sa => sa.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
