using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class OpenAIModerationService : IOpenAIModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIModerationService> _logger;
        private readonly IRedisService _cache;
        private readonly string _apiKey;
        private const string OPENAI_MODERATION_URL = "https://api.openai.com/v1/moderations";
        private const int CACHE_DURATION_HOURS = 24; // Cache kết quả 24h

        public OpenAIModerationService(
            IConfiguration configuration,
            ILogger<OpenAIModerationService> logger,
            IRedisService _redisService)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _cache = _redisService;

            _apiKey = configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API Key not configured");

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<OpenAIModerationResult> ModerateAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new OpenAIModerationResult { Flagged = false };
            }

            // Kiểm tra cache trước
            var cacheKey = GenerateCacheKey(content);
            var cachedResult = await GetCachedResultAsync(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("OpenAI Moderation result retrieved from cache");
                return cachedResult;
            }

            try
            {
                // Gọi OpenAI Moderation API
                var requestBody = new
                {
                    input = content,
                    model = "text-moderation-latest" // Hoặc "text-moderation-stable"
                };

                var response = await _httpClient.PostAsJsonAsync(OPENAI_MODERATION_URL, requestBody);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var moderationResponse = JsonSerializer.Deserialize<OpenAIModerationResponse>(jsonResponse);

                if (moderationResponse?.Results == null || !moderationResponse.Results.Any())
                {
                    _logger.LogWarning("OpenAI Moderation returned empty results");
                    return new OpenAIModerationResult { Flagged = false };
                }

                var firstResult = moderationResponse.Results[0];
                var result = new OpenAIModerationResult
                {
                    Flagged = firstResult.Flagged,
                    Categories = firstResult.Categories ?? new Dictionary<string, bool>(),
                    RawResponse = jsonResponse
                };

                // Cache kết quả
                await CacheResultAsync(cacheKey, result);

                _logger.LogInformation("OpenAI Moderation check completed: Flagged={Flagged}", result.Flagged);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling OpenAI Moderation API");
                // Nếu API lỗi, trả về không flagged để không block review
                return new OpenAIModerationResult { Flagged = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OpenAI Moderation");
                return new OpenAIModerationResult { Flagged = false };
            }
        }

        private string GenerateCacheKey(string content)
        {
            // Tạo hash để làm cache key
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            var hash = Convert.ToBase64String(hashBytes);
            return $"moderation:{hash}";
        }

        private async Task<OpenAIModerationResult?> GetCachedResultAsync(string cacheKey)
        {
            try
            {
                var cached = await _cache.GetAsync<OpenAIModerationResult>(cacheKey);
                return cached;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving moderation result from cache");
            }
            return null;
        }

        private async Task CacheResultAsync(string cacheKey, OpenAIModerationResult result)
        {
            try
            {
                await _cache.SetAsync<OpenAIModerationResult>(cacheKey, result, CacheKeys.Expiration.OpenAIModerationResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error caching moderation result");
            }
        }

        #region OpenAI Response DTOs
        private class OpenAIModerationResponse
        {
            public string? Id { get; set; }
            public string? Model { get; set; }
            public List<ModerationResult>? Results { get; set; }
        }

        private class ModerationResult
        {
            public bool Flagged { get; set; }
            public Dictionary<string, bool>? Categories { get; set; }
            public Dictionary<string, double>? CategoryScores { get; set; }
        }
        #endregion
    }
}
