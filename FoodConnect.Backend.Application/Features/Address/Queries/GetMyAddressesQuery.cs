using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.User;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Queries
{
    public class GetMyAddressesQuery : IRequest<BaseResponse<List<AddressResponse>>>
    {
    }
}
