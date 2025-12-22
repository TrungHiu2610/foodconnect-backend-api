using FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;
using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Commons.Interfaces;

public interface IProductRetrievalService
{
    Task<List<Product>> VectorSearchAsync(string query, int limit = 15, double minSimilarity = 0.3);
    Task<List<Product>> FullTextSearchAsync(UserIntent intent, int limit = 15);
    Task<List<Product>> MetadataFilterAsync(UserIntent intent, int limit = 20);
    Task<List<Product>> HybridSearchAsync(string query, UserIntent intent, int maxResults = 50);
}
