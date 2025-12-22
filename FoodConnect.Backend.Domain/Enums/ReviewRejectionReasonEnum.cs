namespace FoodConnect.Backend.Domain.Enums
{
    public enum ReviewRejectionReasonEnum
    {
        None = 0,
        ToxicKeyword = 1,           // Chứa từ khóa toxic
        OpenAIModeration = 2,       // Bị OpenAI Moderation flag
        TooShort = 3,               // Quá ngắn
        NoFoodContext = 4,          // Không có ngữ cảnh món ăn
        DuplicateContent = 5,       // Nội dung trùng lặp
        UserSpamming = 6            // User spam trong thời gian ngắn
    }
}
