using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique()
                .HasFilter("[ProductId] IS NOT NULL");

            builder.HasIndex(w => new { w.UserId, w.ShopId })
                .IsUnique()
                .HasFilter("[ShopId] IS NOT NULL");

            builder.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Shop)
                .WithMany()
                .HasForeignKey(w => w.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(w => w.Type)
                .IsRequired();

            builder.Property(w => w.NotificationEnabled)
                .HasDefaultValue(true);
        }
    }
}
