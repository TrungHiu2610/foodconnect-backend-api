using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ConfirmDeliveryWithProofCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid OrderId { get; set; }
        public IFormFile DeliveryProofImage { get; set; } = null!; // Required photo proof
    }
}
