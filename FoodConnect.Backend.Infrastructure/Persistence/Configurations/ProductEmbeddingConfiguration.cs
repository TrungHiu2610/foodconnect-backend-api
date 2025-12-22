using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations;

public class ProductEmbeddingConfiguration : IEntityTypeConfiguration<ProductEmbedding>
{
    public void Configure(EntityTypeBuilder<ProductEmbedding> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmbeddingContent)
            .IsRequired()
            .HasColumnType("text");

        // For pgvector, use: .HasColumnType("vector(768)")
        builder.Property(e => e.Embedding)
            .IsRequired()
            .HasColumnType("vector(768)");

        builder.Property(e => e.LastSimilarityScore)
            .HasColumnType("double precision");

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProductId)
            .IsUnique();

        // TODO: Add ivfflat index later for better performance
        // builder.HasIndex(e => e.Embedding)
        //     .HasMethod("ivfflat")
        //     .HasOperators("vector_cosine_ops");
    }
}
