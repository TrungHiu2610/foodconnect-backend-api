using Pgvector;

namespace FoodConnect.Backend.Domain.Entities;

public class ProductEmbedding : BaseEntity
{
    public Guid ProductId { get; set; }
    public string EmbeddingContent { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = new Vector(new float[768]);
    public double? LastSimilarityScore { get; set; }
    
    public virtual Product Product { get; set; } = null!;
}
