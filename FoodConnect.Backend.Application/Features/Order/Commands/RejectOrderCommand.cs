using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class RejectOrderCommand : IRequest<BaseResponse<OrderDetailDto>>
    {
        public Guid OrderId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
