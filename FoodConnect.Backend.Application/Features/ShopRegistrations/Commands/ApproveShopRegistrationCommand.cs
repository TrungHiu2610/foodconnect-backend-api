using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class ApproveShopRegistrationCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid ShopRegistrationId { get; set; }
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
