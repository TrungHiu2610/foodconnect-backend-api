using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class SetDefaultAddressCommand : IRequest<BaseResponse<object>>
    {
        public Guid Id { get; set; }
    }
}
