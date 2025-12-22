using FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Infrastructure.Services;

public class ProductRetrievalService : IProductRetrievalService
{
    private readonly IProductEmbeddingRepository _embeddingRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IGeminiAIService _geminiService;
    private readonly ILogger<ProductRetrievalService> _logger;

    public ProductRetrievalService(
        IProductEmbeddingRepository embeddingRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IGeminiAIService geminiService,
        ILogger<ProductRetrievalService> logger)
    {
        _embeddingRepository = embeddingRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<List<Product>> VectorSearchAsync(string query, int limit = 15, double minSimilarity = 0.3)
    {
        try
        {
            // Generate query embedding
            var queryEmbedding = await _geminiService.GenerateEmbeddingAsync(query);

            // Search in vector database
            var embeddings = await _embeddingRepository.VectorSearchAsync(queryEmbedding, limit, minSimilarity);

            if (!embeddings.Any())
            {
                _logger.LogInformation("No products found in vector search for query: {Query}", query);
                return new List<Product>();
            }

            // Load full product details
            var productIds = embeddings.Select(e => e.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds);

            return products.Where(p => p.IsAvailable && p.Status == ProductStatusEnum.Active).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in vector search for query: {Query}", query);
            return new List<Product>();
        }
    }

    public async Task<List<Product>> FullTextSearchAsync(UserIntent intent, int limit = 15)
    {
        try
        {
            if (!intent.Keywords.Any())
            {
                return new List<Product>();
            }

            var products = await _productRepository.SearchByKeywordsAsync(intent.Keywords, limit);
            return products.Where(p => p.IsAvailable && p.Status == ProductStatusEnum.Active).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in full-text search");
            return new List<Product>();
        }
    }

    public async Task<List<Product>> MetadataFilterAsync(UserIntent intent, int limit = 20)
    {
        try
        {
            var query = _productRepository.GetQueryable();

            // Filter by active and available
            query = query.Where(p => p.Status == ProductStatusEnum.Active && p.IsAvailable);

            // Filter by categories
            if (intent.Categories.Any())
            {
                var categoryIds = await GetCategoryIdsByNamesAsync(intent.Categories);
                if (categoryIds.Any())
                {
                    query = query.Where(p => categoryIds.Contains(p.CategoryId));
                }
            }

            // Filter by price range
            if (!string.IsNullOrEmpty(intent.PriceRange))
            {
                var parts = intent.PriceRange.Split('-');
                if (parts.Length == 2 && decimal.TryParse(parts[0], out var minPrice) && decimal.TryParse(parts[1], out var maxPrice))
                {
                    query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
                }
            }

            // TODO: Add more filters when nutrition fields are added to Product entity
            // - Calories constraint
            // - Nutrition focus (high-protein, low-carb)
            // - Dietary restrictions (stored as tags)
            // - Allergens

            var products = query.Take(limit).ToList();
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in metadata filtering");
            return new List<Product>();
        }
    }

    public async Task<List<Product>> HybridSearchAsync(string query, UserIntent intent, int maxResults = 50)
    {
        try
        {
            _logger.LogInformation("Starting hybrid search for query: {Query}", query);

            // Run all three search methods in parallel
            var vectorTask = VectorSearchAsync(query, limit: 15, minSimilarity: 0.3);
            var fullTextTask = FullTextSearchAsync(intent, limit: 15);
            var metadataTask = MetadataFilterAsync(intent, limit: 20);

            await Task.WhenAll(vectorTask, fullTextTask, metadataTask);

            var vectorResults = await vectorTask;
            var fullTextResults = await fullTextTask;
            var metadataResults = await metadataTask;

            _logger.LogInformation("Search results - Vector: {V}, FullText: {F}, Metadata: {M}",
                vectorResults.Count, fullTextResults.Count, metadataResults.Count);

            // Combine and deduplicate
            var allProducts = new Dictionary<Guid, Product>();

            foreach (var product in vectorResults)
            {
                allProducts[product.Id] = product;
            }

            foreach (var product in fullTextResults)
            {
                if (!allProducts.ContainsKey(product.Id))
                {
                    allProducts[product.Id] = product;
                }
            }

            foreach (var product in metadataResults)
            {
                if (!allProducts.ContainsKey(product.Id))
                {
                    allProducts[product.Id] = product;
                }
            }

            var uniqueProducts = allProducts.Values.Take(maxResults).ToList();
            _logger.LogInformation("Hybrid search returned {Count} unique products", uniqueProducts.Count);

            return uniqueProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in hybrid search");
            return new List<Product>();
        }
    }

    private async Task<List<Guid>> GetCategoryIdsByNamesAsync(List<string> categoryNames)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories
            .Where(c => categoryNames.Any(name => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
            .Select(c => c.Id)
            .ToList();
    }
}
