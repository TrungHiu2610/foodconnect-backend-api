using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetPromotionDetailQuery : IRequest<BaseResponse<PromotionResponse>>
    {
        public Guid PromotionId { get; set; }
    }
}
