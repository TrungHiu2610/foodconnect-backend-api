using FoodConnect.Backend.Domain.Entities;
using Pgvector;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IProductEmbeddingRepository : IBaseRepository<ProductEmbedding>
{
    Task<ProductEmbedding?> GetByProductIdAsync(Guid productId);
    Task<List<ProductEmbedding>> VectorSearchAsync(Vector queryEmbedding, int limit = 15, double minSimilarity = 0.3);
    Task<bool> ExistsForProductAsync(Guid productId);
}
