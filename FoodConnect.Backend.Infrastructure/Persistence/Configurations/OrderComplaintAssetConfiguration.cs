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
    public class OrderComplaintAssetConfiguration : IEntityTypeConfiguration<OrderComplaintAsset>
    {
        public void Configure(EntityTypeBuilder<OrderComplaintAsset> builder)
        {
            builder.HasKey(oca => oca.Id);
            builder.Property(oca => oca.Id).ValueGeneratedOnAdd();

            builder.Property(oca => oca.AssetUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(oca => oca.AssetType)
                .HasConversion<int>()
                .IsRequired();

            builder.HasOne(oca => oca.OrderComplaint)
                .WithMany(o => o.Assets)
                .HasForeignKey(sa => sa.OrderComplaintId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
