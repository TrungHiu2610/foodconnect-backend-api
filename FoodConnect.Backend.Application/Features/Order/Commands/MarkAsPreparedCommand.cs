using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class MarkAsPreparedCommand : IRequest<BaseResponse<OrderDetailDto>>
    {
        public Guid OrderId { get; set; }
    }
}
