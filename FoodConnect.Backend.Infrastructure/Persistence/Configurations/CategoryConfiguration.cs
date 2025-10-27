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
                   .HasForeignKey(c => c.ParentId);
            builder.HasMany(c => c.Products)
                     .WithOne(p => p.Category)
                     .HasForeignKey(p => p.CategoryId)
                     .IsRequired();

            // seed data
            builder.HasData(
                new Category { Id = Guid.Parse("4286ba6a-3d40-46be-8539-237190c067b6"), Name = "Fruits", Description = "Fresh fruits", ImageUrl = "https://example.com/images/fruits.jpg", IsActive = true, DeliveryType = DeliveryTypeEnum.Standard },
                new Category { Id = Guid.Parse("2fa4cb76-edee-44e3-95e2-1f841b27929a"), Name = "Vegetables", Description = "Fresh vegetables", ImageUrl = "https://example.com/images/vegetables.jpg", IsActive = true, DeliveryType = DeliveryTypeEnum.Express },
                new Category { Id = Guid.Parse("ac8e66b0-4fbc-4c97-ad4b-427eab61db1b"), Name = "Dairy", Description = "Dairy products", ImageUrl = "https://example.com/images/dairy.jpg", IsActive = true, DeliveryType = DeliveryTypeEnum.Standard },
                new Category { Id = Guid.Parse("30d76555-7ff9-41d5-a11e-92a347f725bf"), Name = "Citrus Fruits", Description = "Fresh citrus fruits", ImageUrl = "https://example.com/images/citrus.jpg", IsActive = true, DeliveryType = DeliveryTypeEnum.Express, ParentId = Guid.Parse("4286ba6a-3d40-46be-8539-237190c067b6") } ,
                new Category { Id = Guid.Parse("4cf8d54f-d15c-4f02-b3a4-c40c5206b624"), Name = "Berries", Description = "Fresh berries", ImageUrl = "https://example.com/images/berries.jpg", IsActive = true, DeliveryType = DeliveryTypeEnum.Standard, ParentId = Guid.Parse("4286ba6a-3d40-46be-8539-237190c067b6") }
            );
        }
    }
}
