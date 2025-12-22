using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class ReviewModerationService : IReviewModerationService
    {
        private readonly IToxicKeywordFilterService _toxicKeywordFilter;
        private readonly IOpenAIModerationService _openAIModeration;
        private readonly ISpamDetectionService _spamDetection;
        private readonly ILogger<ReviewModerationService> _logger;

        public ReviewModerationService(
            IToxicKeywordFilterService toxicKeywordFilter,
            IOpenAIModerationService openAIModeration,
            ISpamDetectionService spamDetection,
            ILogger<ReviewModerationService> logger)
        {
            _toxicKeywordFilter = toxicKeywordFilter;
            _openAIModeration = openAIModeration;
            _spamDetection = spamDetection;
            _logger = logger;
        }

        public async Task<ModerationResult> ModerateReviewAsync(string content, Guid userId, Guid productId)
        {
            _logger.LogInformation("Starting review moderation for user {UserId}, product {ProductId}", userId, productId);

            // TẦNG 1: Kiểm tra Toxic Keyword
            var toxicKeywordResult = await _toxicKeywordFilter.CheckAsync(content);
            if (toxicKeywordResult.IsToxic)
            {
                _logger.LogWarning("Review blocked by toxic keyword filter. Matched keywords: {Keywords}",
                    string.Join(", ", toxicKeywordResult.MatchedKeywords));

                return new ModerationResult
                {
                    Status = ReviewStatusEnum.Toxic,
                    RejectionReason = ReviewRejectionReasonEnum.ToxicKeyword,
                    Details = $"Chứa từ khóa độc hại: {string.Join(", ", toxicKeywordResult.MatchedKeywords)}"
                };
            }

            // TẦNG 2: Kiểm tra OpenAI Moderation (chỉ gọi nếu tầng 1 pass)
            try
            {
                var openAIResult = await _openAIModeration.ModerateAsync(content);
                if (openAIResult.Flagged)
                {
                    var flaggedCategories = openAIResult.Categories
                        .Where(c => c.Value)
                        .Select(c => c.Key)
                        .ToList();

                    _logger.LogWarning("Review blocked by OpenAI Moderation. Categories: {Categories}",
                        string.Join(", ", flaggedCategories));

                    return new ModerationResult
                    {
                        Status = ReviewStatusEnum.Toxic,
                        RejectionReason = ReviewRejectionReasonEnum.OpenAIModeration,
                        Details = $"Vi phạm chính sách nội dung: {string.Join(", ", flaggedCategories)}"
                    };
                }
            }
            catch (Exception ex)
            {
                // Nếu OpenAI lỗi, log và tiếp tục (không block review)
                _logger.LogError(ex, "OpenAI Moderation failed, continuing with spam detection");
            }

            // TẦNG 3: Kiểm tra Spam/Fake Review
            var spamResult = await _spamDetection.CheckAsync(content, userId, productId);
            if (spamResult.IsSpam)
            {
                _logger.LogWarning("Review blocked by spam detection. Rules violated: {Rules}",
                    string.Join(", ", spamResult.ViolatedRules));

                // Xác định lý do spam chính
                var primaryReason = DetermineSpamReason(spamResult.ViolatedRules);

                return new ModerationResult
                {
                    Status = ReviewStatusEnum.Spam,
                    RejectionReason = primaryReason,
                    Details = string.Join("; ", spamResult.ViolatedRules)
                };
            }

            // Review hợp lệ (cả positive và negative đều được chấp nhận)
            _logger.LogInformation("Review approved for user {UserId}, product {ProductId}", userId, productId);
            return new ModerationResult
            {
                Status = ReviewStatusEnum.Approved,
                RejectionReason = null,
                Details = "Review đã được phê duyệt"
            };
        }

        private ReviewRejectionReasonEnum DetermineSpamReason(List<string> violatedRules)
        {
            if (violatedRules.Any(r => r.Contains("too short", StringComparison.OrdinalIgnoreCase)))
                return ReviewRejectionReasonEnum.TooShort;

            if (violatedRules.Any(r => r.Contains("no food context", StringComparison.OrdinalIgnoreCase)))
                return ReviewRejectionReasonEnum.NoFoodContext;

            if (violatedRules.Any(r => r.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
                return ReviewRejectionReasonEnum.DuplicateContent;

            if (violatedRules.Any(r => r.Contains("spamming", StringComparison.OrdinalIgnoreCase)))
                return ReviewRejectionReasonEnum.UserSpamming;

            return ReviewRejectionReasonEnum.None;
        }
    }
}
