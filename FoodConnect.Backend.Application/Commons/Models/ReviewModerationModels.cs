using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Models
{
    public class ModerationResult
    {
        public ReviewStatusEnum Status { get; set; }
        public ReviewRejectionReasonEnum? RejectionReason { get; set; }
        public string? Details { get; set; }
        public bool IsApproved => Status == ReviewStatusEnum.Approved;
        public bool IsToxic => Status == ReviewStatusEnum.Toxic;
        public bool IsSpam => Status == ReviewStatusEnum.Spam;
    }
    public class ToxicKeywordCheckResult
    {
        public bool IsToxic { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
    }
    public class OpenAIModerationResult
    {
        public bool Flagged { get; set; }
        public Dictionary<string, bool> Categories { get; set; } = new();
        public string? RawResponse { get; set; }
    }
    public class SpamCheckResult
    {
        public bool IsSpam { get; set; }
        public List<string> ViolatedRules { get; set; } = new();
    }
}
