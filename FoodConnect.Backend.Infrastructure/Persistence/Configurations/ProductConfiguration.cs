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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        //public class Product : BaseEntity
        //{
        //    public string Name { get; set; }
        //    public string? Description { get; set; }
        //    public decimal Price { get; set; }
        //    public string Ingredients { get; set; }
        //    public string Weight { get; set; }
        //    public string ExpiryDate { get; set; }
        //    public string StorageInstructions { get; set; }
        //    public string UsageInstructions { get; set; }
        //    public ProductStatusEnum Status { get; set; } = ProductStatusEnum.Draft;
        //    public bool IsAvailable { get; set; } = true;
        //    public int? StockQuantity { get; set; }

        //    public Guid CategoryId { get; set; }
        //    public virtual Category Category { get; set; }
        //    public Guid ShopId { get; set; }
        //    public virtual Shop Shop { get; set; }
        //    public ICollection<ProductAsset> ProductAssets { get; set; } = new List<ProductAsset>();
        //}
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.Description)
                .HasMaxLength(1000);
            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(p => p.Ingredients)
                .IsRequired()
                .HasMaxLength(1000);
            builder.Property(p => p.Weight)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(p => p.ExpiryDate)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(p => p.StorageInstructions)
                .IsRequired()
                .HasMaxLength(1000);
            builder.Property(p => p.UsageInstructions)
                .IsRequired()
                .HasMaxLength(1000);
            builder.Property(p=> p.Status)
                .IsRequired()
                .HasDefaultValue(ProductStatusEnum.Draft);
            
            // Ignore calculated fields (not mapped to database)
            builder.Ignore(p => p.CalculatedDistance);
            
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);
            builder.HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(p => p.ProductAssets)
                .WithOne(pa => pa.Product)
                .HasForeignKey(pa => pa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
