using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class UpdatePromotionCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public string? Description { get; set; }
        public IFormFile? CoverImage { get; set; }
        public int? PromotionType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MinimumOrderValue { get; set; }
        public int? MaxUsageCount { get; set; }
        public int? UsagePerCustomer { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ApplicableToAllProducts { get; set; }
        public List<Guid>? ProductIds { get; set; }
    }
}
