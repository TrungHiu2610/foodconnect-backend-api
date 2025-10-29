using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrderDetailQuery : IRequest<BaseResponse<OrderDetailDto>>
    {
        public Guid OrderId { get; set; }
    }
}
