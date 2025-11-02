using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateAddressCommandHandler(
            IAddressRepository addressRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(
            CreateAddressCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.IsDefault)
                {
                    await _addressRepository.UnsetAllDefaultAddressesAsync(userId.Value);
                }

                var address = new Domain.Entities.Address
                {
                    UserId = userId.Value,
                    RecipientName = request.RecipientName,
                    PhoneNumber = request.PhoneNumber,
                    Street = request.Street,
                    Ward = request.Ward,
                    District = request.District,
                    City = request.City,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsDefault = request.IsDefault,
                    Note = request.Note,
                    AddressType = (AddressTypeEnum)request.AddressType
                };

                await _addressRepository.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction);

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = address.Id },
                    "Address created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Failed to create address: {ex.Message}");
            }
        }
    }
}
