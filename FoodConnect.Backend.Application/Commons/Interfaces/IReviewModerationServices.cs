using FoodConnect.Backend.Application.Commons.Models;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IToxicKeywordFilterService
    {
        Task<ToxicKeywordCheckResult> CheckAsync(string content);
    }

    public interface IOpenAIModerationService
    {
        Task<OpenAIModerationResult> ModerateAsync(string content);
    }

    public interface ISpamDetectionService
    {
        Task<SpamCheckResult> CheckAsync(string content, Guid userId, Guid productId);
    }

    public interface IReviewModerationService
    {
        Task<ModerationResult> ModerateReviewAsync(string content, Guid userId, Guid productId);
    }
}
