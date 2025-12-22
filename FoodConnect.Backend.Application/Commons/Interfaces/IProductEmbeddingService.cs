namespace FoodConnect.Backend.Application.Commons.Interfaces;

public interface IProductEmbeddingService
{
    Task GenerateProductEmbeddingAsync(Guid productId);
    Task GenerateAllProductEmbeddingsAsync();
    Task UpdateProductEmbeddingAsync(Guid productId);
    Task DeleteProductEmbeddingAsync(Guid productId);
    string BuildEmbeddingContent(Domain.Entities.Product product);
}
