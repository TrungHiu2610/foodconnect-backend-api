using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class CreateAddressCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string? District { get; set; }
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? Note { get; set; }
        public int AddressType { get; set; } = (int)AddressTypeEnum.Home;
    }
}
