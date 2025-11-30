using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ConfirmStandardDeliveryCommand : IRequest<BaseResponse<OrderDetailDto>>
    {
        public Guid OrderId { get; set; }
        public IFormFile TrackingProofImage { get; set; } = null!;
    }
}
