using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.User;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Queries
{
    public class GetMyAddressesQueryHandler : IRequestHandler<GetMyAddressesQuery, BaseResponse<List<AddressResponse>>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyAddressesQueryHandler(
            IAddressRepository addressRepository,
            ICurrentUserService currentUserService)
        {
            _addressRepository = addressRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<List<AddressResponse>>> Handle(
            GetMyAddressesQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<AddressResponse>>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            var addresses = await _addressRepository.GetByUserIdAsync(userId.Value);

            var response = addresses.Select(a => new AddressResponse
            {
                Id = a.Id,
                RecipientName = a.RecipientName,
                PhoneNumber = a.PhoneNumber,
                Street = a.Street,
                Ward = a.Ward,
                District = a.District,
                City = a.City,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                IsDefault = a.IsDefault,
                Note = a.Note,
                AddressType = a.AddressType.ToString()
            }).ToList();

            return result.BuildSuccess(response, "Addresses retrieved successfully");
        }
    }
}
