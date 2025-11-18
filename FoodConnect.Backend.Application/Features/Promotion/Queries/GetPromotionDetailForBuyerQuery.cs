using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetPromotionDetailForBuyerQuery : IRequest<BaseResponse<PromotionDetailForBuyerResponse>>
    {
        public Guid PromotionId { get; set; }
    }
}
