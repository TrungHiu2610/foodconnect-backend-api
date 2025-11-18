using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetMyPromotionsQuery : IRequest<BaseResponse<List<PromotionListResponse>>>
    {
        public int? Status { get; set; }
    }
}
