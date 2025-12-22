using FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;
using Pgvector;

namespace FoodConnect.Backend.Application.Commons.Interfaces;

public interface IGeminiAIService
{
    Task<Vector> GenerateEmbeddingAsync(string text);
    Task<UserIntent> ExtractIntentAsync(string userMessage, List<string>? conversationHistory = null);
    Task<List<RankedProduct>> RerankProductsAsync(string userQuestion, UserIntent intent, List<RankedProduct> products, int topK = 5);
    Task<string> GenerateResponseAsync(string userQuestion, UserIntent intent, List<RankedProduct> products, List<string>? conversationHistory = null);
}
