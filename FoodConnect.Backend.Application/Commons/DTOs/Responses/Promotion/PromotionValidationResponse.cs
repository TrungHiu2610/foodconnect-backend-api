namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion
{
    public class PromotionValidationResponse
    {
        public bool IsValid { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? PromotionId { get; set; }
        public string? PromotionName { get; set; }
    }
}
