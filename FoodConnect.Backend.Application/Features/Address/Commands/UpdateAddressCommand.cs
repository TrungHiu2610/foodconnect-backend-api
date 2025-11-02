using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class UpdateAddressCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid Id { get; set; }
        public string? RecipientName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool? IsDefault { get; set; }
        public string? Note { get; set; }
        public int? AddressType { get; set; }
    }
}
