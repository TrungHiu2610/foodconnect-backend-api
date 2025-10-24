using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class ApproveShopRegistrationCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ShopRegistrationId { get; set; }
    }
}
