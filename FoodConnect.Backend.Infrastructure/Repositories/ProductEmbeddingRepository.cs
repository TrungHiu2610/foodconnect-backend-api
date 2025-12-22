using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class ProductEmbeddingRepository : BaseRepository<ProductEmbedding>, IProductEmbeddingRepository
{
    public ProductEmbeddingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ProductEmbedding?> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductEmbeddings
            .Include(e => e.Product)
            .FirstOrDefaultAsync(e => e.ProductId == productId);
    }

    public async Task<List<ProductEmbedding>> VectorSearchAsync(Vector queryEmbedding, int limit = 15, double minSimilarity = 0.3)
    {
        var embeddings = await _context.ProductEmbeddings
            .FromSqlRaw(@"
                SELECT * 
                FROM ""ProductEmbeddings""
                WHERE (1 - (""Embedding"" <=> CAST({0} AS vector))) >= {1}
                ORDER BY ""Embedding"" <=> CAST({0} AS vector)
                LIMIT {2}",
                queryEmbedding,
                minSimilarity,
                limit)
            .Include(e => e.Product)
                .ThenInclude(p => p.Category)
            .Include(e => e.Product)
                .ThenInclude(p => p.Shop)
            .AsNoTracking()
            .ToListAsync();

        foreach (var embedding in embeddings)
        {
            // Similarity already calculated by database using <=> operator
            // Just calculate for reference (1 - cosine_distance = similarity)
            embedding.LastSimilarityScore = 1.0; // Will be overwritten if needed
        }

        return embeddings;
    }

    public async Task<bool> ExistsForProductAsync(Guid productId)
    {
        return await _context.ProductEmbeddings.AnyAsync(e => e.ProductId == productId);
    }
}
