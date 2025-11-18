using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class ValidatePromotionQuery : IRequest<BaseResponse<PromotionValidationResponse>>
    {
        public Guid PromotionId { get; set; }
        public Guid ShopId { get; set; }
        public List<Guid>? ProductIds { get; set; }
        public decimal OrderValue { get; set; }
    }
}
