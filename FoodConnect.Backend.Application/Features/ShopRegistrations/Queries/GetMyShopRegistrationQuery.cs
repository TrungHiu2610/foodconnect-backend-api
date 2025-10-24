using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.ShopRegistration;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Queries
{
    public class GetMyShopRegistrationQuery : IRequest<BaseResponse<ShopRegistrationResponse>>
    {
    }
}
