using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetActivePromotionsByShopQuery : IRequest<BaseResponse<List<PromotionListResponse>>>
    {
        public Guid ShopId { get; set; }
    }
}
