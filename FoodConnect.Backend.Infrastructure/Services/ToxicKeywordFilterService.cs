using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using System.Globalization;
using System.Text;

namespace FoodConnect.Backend.Infrastructure.Services
{
    /// TẦNG 1: Lọc review toxic bằng danh sách từ khóa tiếng Việt
    public class ToxicKeywordFilterService : IToxicKeywordFilterService
    {
        private static readonly HashSet<string> _toxicKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Từ chửi thề phổ biến
            "dm", "đm", "dmm", "đmm", "dcm", "đcm",
            "deo", "đéo", "đ éo", "d éo",
            "vcl", "vãi", "vai", "vl",
            "cac", "cặc", "cc",
            "lon", "lồn", "lol",
            "dit", "đit", "đ ịt", "địt",
            "buoi", "bươi", "buồi",
            "catcut", "cặt cụt",
            
            // Từ xúc phạm
            "ngu", "nguu", "ngu si", "ngốc",
            "óc chó", "oc cho", "đầu bò", "dau bo",
            "chó má", "cho ma", "chó mẹ", "cho me",
            "đồ ngu", "do ngu", "thằng ngu", "thang ngu",
            "con ngu", "đồ ngốc", "do ngoc",
            
            // Từ độc hại khác
            "rác rưởi", "rac ruoi", "rác", "rac",
            "bẩn thỉu", "ban thiu", "bẩn", "ban",
            "thối", "thoi", "thối tha", "thoi tha",
            "tởm", "tom", "kinh tởm", "kinh tom",
            "cứt", "cut", "như cứt", "nhu cut",
            "shit", "fuck", "damn", "asshole", "bitch"
        };

        private static readonly HashSet<string> _normalizedKeywords;

        static ToxicKeywordFilterService()
        {
            _normalizedKeywords = new HashSet<string>(
                _toxicKeywords.Select(k => RemoveDiacritics(k.ToLowerInvariant()))
            );
        }

        public Task<ToxicKeywordCheckResult> CheckAsync(string content)
        {
            var result = new ToxicKeywordCheckResult();

            if (string.IsNullOrWhiteSpace(content))
            {
                return Task.FromResult(result);
            }

            // Normalize content: lowercase và bỏ dấu
            var normalizedContent = RemoveDiacritics(content.ToLowerInvariant());

            // Kiểm tra từng keyword
            var matchedKeywords = new List<string>();
            foreach (var keyword in _toxicKeywords)
            {
                var normalizedKeyword = RemoveDiacritics(keyword.ToLowerInvariant());
                
                // Kiểm tra cả content gốc và normalized
                if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    normalizedContent.Contains(normalizedKeyword))
                {
                    matchedKeywords.Add(keyword);
                }
            }

            if (matchedKeywords.Any())
            {
                result.IsToxic = true;
                result.MatchedKeywords = matchedKeywords;
            }

            return Task.FromResult(result);
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
