using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class ApproveShopCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ShopId { get; set; }
    }
}
