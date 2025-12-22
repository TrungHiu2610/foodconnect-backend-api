using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Infrastructure.Services;

public class ProductEmbeddingService : IProductEmbeddingService
{
    private readonly IProductEmbeddingRepository _embeddingRepository;
    private readonly IProductRepository _productRepository;
    private readonly IGeminiAIService _geminiService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductEmbeddingService> _logger;

    public ProductEmbeddingService(
        IProductEmbeddingRepository embeddingRepository,
        IProductRepository productRepository,
        IGeminiAIService geminiService,
        IUnitOfWork unitOfWork,
        ILogger<ProductEmbeddingService> logger)
    {
        _embeddingRepository = embeddingRepository;
        _productRepository = productRepository;
        _geminiService = geminiService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task GenerateProductEmbeddingAsync(Guid productId)
    {
        // Check if embedding already exists
        var existingEmbedding = await _embeddingRepository.GetByProductIdAsync(productId);
        if (existingEmbedding != null)
        {
            _logger.LogInformation("Embedding already exists for product {ProductId}", productId);
            return;
        }

        await CreateEmbeddingAsync(productId);
    }

    public async Task GenerateAllProductEmbeddingsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var activeProducts = products.Where(p => p.IsAvailable).ToList();

        _logger.LogInformation("Generating embeddings for {Count} products", activeProducts.Count);

        foreach (var product in activeProducts)
        {
            try
            {
                var exists = await _embeddingRepository.ExistsForProductAsync(product.Id);
                if (!exists)
                {
                    await CreateEmbeddingAsync(product.Id);
                    await Task.Delay(4000); // Rate limiting: 15 req/min
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for product {ProductId}", product.Id);
            }
        }
    }

    public async Task UpdateProductEmbeddingAsync(Guid productId)
    {
        var existingEmbedding = await _embeddingRepository.GetByProductIdAsync(productId);
        if (existingEmbedding != null)
        {
            _embeddingRepository.Remove(existingEmbedding);
            await _unitOfWork.SaveChangesAsync();
        }

        await CreateEmbeddingAsync(productId);
    }

    public async Task DeleteProductEmbeddingAsync(Guid productId)
    {
        var embedding = await _embeddingRepository.GetByProductIdAsync(productId);
        if (embedding != null)
        {
            _embeddingRepository.Remove(embedding);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public string BuildEmbeddingContent(Product product)
    {
        var parts = new List<string>();

        // Product name (most important)
        parts.Add($"Tên sản phẩm: {product.Name}");

        // Description
        if (!string.IsNullOrEmpty(product.Description))
        {
            parts.Add($"Mô tả: {product.Description}");
        }

        // Category
        if (product.Category != null)
        {
            parts.Add($"Danh mục: {product.Category.Name}");
            if (product.Category.Parent != null)
            {
                parts.Add($"Danh mục cha: {product.Category.Parent.Name}");
            }
        }

        // Ingredients
        if (!string.IsNullOrEmpty(product.Ingredients))
        {
            parts.Add($"Thành phần: {product.Ingredients}");
        }

        // Shop information
        if (product.Shop != null)
        {
            parts.Add($"Cửa hàng: {product.Shop.ShopName}");
            if (product.Shop.Rating.HasValue)
            {
                parts.Add($"Đánh giá shop: {product.Shop.Rating:F1}/5");
            }
        }

        // Price range indicator
        var priceRange = product.Price switch
        {
            < 50000 => "Giá rẻ, dưới 50k",
            < 100000 => "Giá trung bình, 50k-100k",
            < 200000 => "Giá cao, 100k-200k",
            _ => "Giá cao cấp, trên 200k"
        };
        parts.Add(priceRange);

        // Weight
        if (!string.IsNullOrEmpty(product.Weight))
        {
            parts.Add($"Khối lượng: {product.Weight}");
        }

        // Storage and usage instructions (for health/nutrition context)
        if (!string.IsNullOrEmpty(product.StorageInstructions))
        {
            parts.Add($"Bảo quản: {product.StorageInstructions}");
        }

        if (!string.IsNullOrEmpty(product.UsageInstructions))
        {
            parts.Add($"Cách dùng: {product.UsageInstructions}");
        }

        // Expiry date indicator
        if (!string.IsNullOrEmpty(product.ExpiryDate))
        {
            parts.Add($"Hạn sử dụng: {product.ExpiryDate}");
        }

        // Availability
        parts.Add(product.IsAvailable ? "Còn hàng" : "Hết hàng");

        return string.Join(". ", parts);
    }

    private async Task CreateEmbeddingAsync(Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new Exception($"Product {productId} not found");
        }

        var embeddingContent = BuildEmbeddingContent(product);
        var embeddingVector = await _geminiService.GenerateEmbeddingAsync(embeddingContent);

        var productEmbedding = new ProductEmbedding
        {
            ProductId = productId,
            EmbeddingContent = embeddingContent,
            Embedding = embeddingVector
        };

        await _embeddingRepository.AddAsync(productEmbedding);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Generated embedding for product {ProductId}", productId);
    }
}
