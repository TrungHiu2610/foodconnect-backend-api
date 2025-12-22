using FoodConnect.Backend.Application.Commons.DTOs.AIChatbot;
using FoodConnect.Backend.Application.Commons.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pgvector;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodConnect.Backend.Infrastructure.Services;

public class GeminiAIService : IGeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly string _apiKey;
    private const string EmbeddingModel = "text-embedding-004";
    private const string GenerationModel = "gemini-2.0-flash-exp";

    public GeminiAIService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("GeminiAI:ApiKey not configured");
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";
            
            var requestBody = new
            {
                content = new
                {
                    parts = new[] { new { text } }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>();
            
            if (result?.Embedding?.Values == null || result.Embedding.Values.Length == 0)
            {
                throw new Exception("Empty embedding response from Gemini");
            }

            _logger.LogInformation("Generated embedding with {Dimensions} dimensions", result.Embedding.Values.Length);
            return new Vector(result.Embedding.Values);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            throw;
        }
    }

    public async Task<UserIntent> ExtractIntentAsync(string userMessage, List<string>? conversationHistory = null)
    {
        try
        {
            var systemPrompt = @"Bạn là chuyên gia phân tích ý định người dùng trên sàn thương mại điện tử thực phẩm FoodConnect.

Nhiệm vụ: Phân tích câu hỏi và trả về JSON có cấu trúc sau (KHÔNG bao gồm markdown, CHÍNH XÁC JSON):

{
  ""categories"": [""Snacks"", ""Healthy Food""],
  ""keywords"": [""ăn vặt"", ""healthy"", ""protein""],
  ""dietaryRestrictions"": [""vegetarian"", ""gluten-free""],
  ""nutritionFocus"": ""high-protein"",
  ""priceRange"": ""50000-100000"",
  ""mealType"": ""snack"",
  ""caloriesConstraint"": ""< 200"",
  ""allergens"": [""nuts""],
  ""specialRequests"": [""organic"", ""local""],
  ""location"": null
}

Quy tắc:
- categories: Danh mục sản phẩm (Snacks, Healthy Food, Breakfast, Lunch, Dinner, Beverages, Desserts)
- keywords: Từ khóa quan trọng từ câu hỏi
- dietaryRestrictions: vegetarian, vegan, gluten-free, dairy-free, sugar-free, keto
- nutritionFocus: high-protein, low-carb, low-fat, low-calorie, high-fiber
- priceRange: ""min-max"" (VNĐ) hoặc null
- mealType: breakfast, lunch, dinner, snack
- caloriesConstraint: ""< 200"", ""> 300"", ""100-200"" hoặc null
- allergens: nuts, dairy, soy, eggs, seafood, gluten
- specialRequests: organic, local, fresh, handmade
- location: Địa điểm (nếu có)

QUAN TRỌNG: Chỉ trả về JSON thuần túy, KHÔNG có ```json hoặc giải thích gì thêm.";

            var userPrompt = $"Câu hỏi người dùng: {userMessage}";
            
            if (conversationHistory != null && conversationHistory.Any())
            {
                userPrompt += $"\n\nLịch sử chat:\n{string.Join("\n", conversationHistory.TakeLast(3))}";
            }

            var responseText = await CallGeminiGenerateAsync(systemPrompt, userPrompt);
            
            // Clean response (remove markdown code blocks if present)
            responseText = responseText.Trim();
            if (responseText.StartsWith("```json"))
            {
                responseText = responseText.Substring(7);
            }
            if (responseText.StartsWith("```"))
            {
                responseText = responseText.Substring(3);
            }
            if (responseText.EndsWith("```"))
            {
                responseText = responseText.Substring(0, responseText.Length - 3);
            }
            responseText = responseText.Trim();

            var intent = JsonSerializer.Deserialize<UserIntent>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return intent ?? new UserIntent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting intent from message: {Message}", userMessage);
            return new UserIntent(); // Return empty intent on error
        }
    }

    public async Task<List<RankedProduct>> RerankProductsAsync(string userQuestion, UserIntent intent, List<RankedProduct> products, int topK = 5)
    {
        try
        {
            if (!products.Any()) return new List<RankedProduct>();

            var systemPrompt = @"Bạn là chuyên gia tư vấn dinh dưỡng và thực phẩm cho FoodConnect.

Nhiệm vụ: Đánh giá và xếp hạng các sản phẩm dựa trên:
1. Độ phù hợp với nhu cầu người dùng (30%)
2. Phù hợp với hạn chế chế độ ăn (25%)
3. Giá cả hợp lý (20%)
4. Đánh giá cửa hàng (15%)
5. Giá trị dinh dưỡng (10%)

Trả về JSON array (KHÔNG bao gồm markdown):
[
  {""id"": ""guid"", ""score"": 9.5, ""reason"": ""Lý do cụ thể""},
  {""id"": ""guid"", ""score"": 8.0, ""reason"": ""Lý do cụ thể""}
]

Score: 0-10 (10 = hoàn hảo)
CHỈ trả về JSON array, không có văn bản khác.";

            var productsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Ingredients,
                p.CategoryName,
                p.ShopName,
                p.ShopRating
            }));

            var intentJson = JsonSerializer.Serialize(intent);
            var userPrompt = $"Câu hỏi: {userQuestion}\n\nYêu cầu: {intentJson}\n\nSản phẩm:\n{productsJson}\n\nChọn top {topK} sản phẩm phù hợp nhất.";

            var responseText = await CallGeminiGenerateAsync(systemPrompt, userPrompt);
            
            // Clean response
            responseText = responseText.Trim();
            if (responseText.StartsWith("```json")) responseText = responseText.Substring(7);
            if (responseText.StartsWith("```")) responseText = responseText.Substring(3);
            if (responseText.EndsWith("```")) responseText = responseText.Substring(0, responseText.Length - 3);
            responseText = responseText.Trim();

            var scoredResults = JsonSerializer.Deserialize<List<ScoredProduct>>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (scoredResults == null || !scoredResults.Any())
            {
                _logger.LogWarning("Empty reranking response, returning top products as-is");
                return products.Take(topK).ToList();
            }

            // Merge scores back to products
            var rankedProducts = new List<RankedProduct>();
            foreach (var scored in scoredResults.Take(topK))
            {
                var product = products.FirstOrDefault(p => p.Id.ToString().Equals(scored.Id, StringComparison.OrdinalIgnoreCase));
                if (product != null)
                {
                    product.Score = scored.Score;
                    product.Reason = scored.Reason;
                    rankedProducts.Add(product);
                }
            }

            return rankedProducts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reranking products");
            return products.Take(topK).ToList(); // Fallback: return first N products
        }
    }

    public async Task<string> GenerateResponseAsync(string userQuestion, UserIntent intent, List<RankedProduct> products, List<string>? conversationHistory = null)
    {
        try
        {
            var systemPrompt = @"Ban la tro ly AI than thien va chuyen nghiep cua FoodConnect - san thuc pham sach.

Phong cach:
- Than thien, nhiet tinh nhu nguoi tu van ban hang
- Tu van dinh duong chinh xac
- Huong dan cu the cach mua hang

Cau truc cau tra loi:
1. Chao hoi ngan gon
2. Gioi thieu 2-3 san pham phu hop nhat:
   - Ten san pham
   - 2-3 uu diem dinh duong/dac diem noi bat
   - Gia ca ro rang
   - Ten quan/shop ban (voi danh gia neu co)
   - Ly do goi y (tai sao phu hop voi nhu cau)
   - Huong dan: ""Xem chi tiet tai [link]""
3. Huong dan mua hang (neu phu hop):
   - ""De dat mua: Click vao link san pham > Them vao gio > Thanh toan""
   - Luu y ve ton kho/giao hang neu can
4. Goi y cau hoi tiep theo

LUU Y QUAN TRONG:
- CHI gioi thieu san pham co trong danh sach
- KHONG bia dat san pham khong ton tai
- Luon de cap ten quan/shop va link de buyer de tim
- Giai thich ro LY DO goi y dua tren nhieu tieu chi (gia, dinh duong, danh gia, phu hop nhu cau)
- Toi da 250 tu";

            var productsInfo = string.Join("\n\n", products.Select((p, i) => 
                $"{i + 1}. **{p.Name}** ({p.Price:N0}d)\n" +
                $"   - Danh muc: {p.CategoryName}\n" +
                $"   - Quan/Shop: {p.ShopName} {(p.ShopRating.HasValue ? $"({p.ShopRating:F1}/5 sao)" : "")}\n" +
                $"   - Mo ta: {p.Description}\n" +
                $"   - LY DO GOI Y: {p.Reason}\n" +
                $"   - Link san pham: /products/{p.Id}\n" +
                $"   - Link quan/shop: /shops/{p.ShopId}"));

            var userPrompt = $"Cau hoi: {userQuestion}\n\nSan pham goi y:\n{productsInfo}";

            if (conversationHistory != null && conversationHistory.Any())
            {
                userPrompt += $"\n\nLich su:\n{string.Join("\n", conversationHistory.TakeLast(3))}";
            }

            var response = await CallGeminiGenerateAsync(systemPrompt, userPrompt);
            return response.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response");
            return "Xin loi, minh gap loi khi xu ly yeu cau. Ban vui long thu lai sau nhe!";
        }
    }

    private async Task<string> CallGeminiGenerateAsync(string systemPrompt, string userPrompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GenerationModel}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = $"{systemPrompt}\n\n{userPrompt}" } }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 2048
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>();
        
        if (result?.Candidates == null || !result.Candidates.Any())
        {
            throw new Exception("Empty response from Gemini");
        }

        return result.Candidates[0].Content.Parts[0].Text;
    }

    // Response models
    private class GeminiEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public EmbeddingData? Embedding { get; set; }
    }

    private class EmbeddingData
    {
        [JsonPropertyName("values")]
        public float[] Values { get; set; } = Array.Empty<float>();
    }

    private class GeminiGenerateResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = new();
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; } = new();
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class ScoredProduct
    {
        public string Id { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
