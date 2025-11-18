namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion
{
    public class ApplicablePromotionResponse
    {
        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; } = string.Empty;
        public int PromotionType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinimumOrderValue { get; set; }
        public decimal OrderValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public bool CanApply { get; set; }
        public string? ReasonCannotApply { get; set; }
    }
}
