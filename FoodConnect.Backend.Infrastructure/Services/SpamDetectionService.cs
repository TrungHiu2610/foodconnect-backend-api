using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class SpamDetectionService : ISpamDetectionService
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly ILogger<SpamDetectionService> _logger;

        private static readonly HashSet<string> _foodKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "bún", "bun", "phở", "pho", "cơm", "com", "bánh", "banh",
            "trà", "tra", "sữa", "sua", "cafe", "cà phê", "ca phe",
            "gà", "ga", "bò", "bo", "heo", "lợn", "lon",
            "tôm", "tom", "cá", "ca", "mực", "muc",
            "rau", "canh", "súp", "sup", "lẩu", "lau",
            "nước", "nuoc", "sốt", "sot", "gia vị", "gia vi",
            "món", "mon", "đồ ăn", "do an", "thức ăn", "thuc an",
            "ngon", "dở", "do", "mặn", "man", "nhạt", "nhat",
            "cay", "ngọt", "ngot", "chua", "đắng", "dang",
            "tươi", "tuoi", "thơm", "thom", "mùi", "mui",
            "nóng", "nong", "lạnh", "lanh", "ấm", "am",
            "giòn", "gion", "mềm", "mem", "dai", "béo", "beo",
            "pizza", "burger", "sandwich", "salad", "pasta",
            "chicken", "beef", "pork", "fish", "rice", "noodle",
            "drink", "juice", "smoothie", "tea", "coffee", "milk"
        };

        private const int MIN_REVIEW_LENGTH = 10; // Tối thiểu 10 ký tự
        private const int DUPLICATE_THRESHOLD = 3; // Xuất hiện >= 3 lần là spam
        private const int SPAM_TIME_WINDOW_MINUTES = 10; // 10 phút
        private const int MAX_REVIEWS_IN_WINDOW = 3; // Tối đa 3 review trong 10 phút

        public SpamDetectionService(
            IProductReviewRepository reviewRepository,
            ILogger<SpamDetectionService> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<SpamCheckResult> CheckAsync(string content, Guid userId, Guid productId)
        {
            var result = new SpamCheckResult();
            var violatedRules = new List<string>();

            if (string.IsNullOrWhiteSpace(content))
            {
                result.IsSpam = true;
                result.ViolatedRules.Add("Empty content");
                return result;
            }

            // Rule 1: Review quá ngắn
            if (await CheckTooShort(content))
            {
                violatedRules.Add("Review too short (< 10 characters)");
            }

            // Rule 2: Không chứa ngữ cảnh món ăn
            if (await CheckNoFoodContext(content))
            {
                violatedRules.Add("No food context found");
            }

            // Rule 3: Nội dung trùng lặp
            var duplicateCount = await CheckDuplicateContent(content);
            if (duplicateCount >= DUPLICATE_THRESHOLD)
            {
                violatedRules.Add($"Duplicate content found ({duplicateCount} times)");
            }

            // Rule 4: User spam trong thời gian ngắn
            var recentReviewCount = await CheckUserSpamming(userId);
            if (recentReviewCount > MAX_REVIEWS_IN_WINDOW)
            {
                violatedRules.Add($"User spamming ({recentReviewCount} reviews in {SPAM_TIME_WINDOW_MINUTES} minutes)");
            }

            // Nếu vi phạm bất kỳ rule nào → spam
            if (violatedRules.Any())
            {
                result.IsSpam = true;
                result.ViolatedRules = violatedRules;
                _logger.LogInformation("Spam detected: {Rules}", string.Join(", ", violatedRules));
            }

            return result;
        }

        /// Rule 1: Kiểm tra review quá ngắn
        private Task<bool> CheckTooShort(string content)
        {
            var cleanContent = Regex.Replace(content, @"\s+", " ").Trim();
            return Task.FromResult(cleanContent.Length < MIN_REVIEW_LENGTH);
        }

        /// Rule 2: Kiểm tra không có ngữ cảnh món ăn
        private Task<bool> CheckNoFoodContext(string content)
        {
            var normalizedContent = RemoveDiacritics(content.ToLowerInvariant());
            
            // Kiểm tra xem có ít nhất 1 từ khóa về món ăn không
            var hasFoodKeyword = _foodKeywords.Any(keyword =>
            {
                var normalizedKeyword = RemoveDiacritics(keyword.ToLowerInvariant());
                return content.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                       normalizedContent.Contains(normalizedKeyword);
            });

            return Task.FromResult(!hasFoodKeyword);
        }

        /// Rule 3: Kiểm tra nội dung trùng lặp
        private async Task<int> CheckDuplicateContent(string content)
        {
            try
            {
                // Normalize content để so sánh
                var normalizedContent = NormalizeForComparison(content);
                
                // Đếm số review có nội dung giống nhau
                var duplicateCount = await _reviewRepository.CountDuplicateReviewsAsync(normalizedContent);
                
                return duplicateCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate content");
                return 0;
            }
        }

        /// Rule 4: Kiểm tra user spam trong thời gian ngắn
        private async Task<int> CheckUserSpamming(Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var timeWindowStart = now.AddMinutes(-SPAM_TIME_WINDOW_MINUTES);
                
                var recentReviewCount = await _reviewRepository.CountUserReviewsInTimeRangeAsync(userId, timeWindowStart, now);
                
                return recentReviewCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user spamming");
                return 0;
            }
        }

        private string NormalizeForComparison(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Lowercase, bỏ dấu, bỏ khoảng trắng thừa, bỏ emoji
            var normalized = RemoveDiacritics(text.ToLowerInvariant());
            normalized = Regex.Replace(normalized, @"\s+", " ");
            normalized = Regex.Replace(normalized, @"[^\w\s]", ""); // Bỏ ký tự đặc biệt
            return normalized.Trim();
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }
    }
}
