using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);
            builder.Property(c => c.ImageUrl).HasMaxLength(500);
            builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(c => c.DeliveryType).IsRequired().HasDefaultValue(DeliveryTypeEnum.Standard);
            builder.HasOne(c => c.Parent)
                   .WithMany()
                   .HasForeignKey(c => c.ParentId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Products)
                     .WithOne(p => p.Category)
                     .HasForeignKey(p => p.CategoryId)
                     .IsRequired();
        }
    }
}
