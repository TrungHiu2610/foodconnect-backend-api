using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetCategoriesByShopQuery : IRequest<BaseResponse<GetListCategoryResponse>>
    {
        public Guid ShopId { get; set; }
    }
}
