using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand, BaseResponse<object>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SetDefaultAddressCommandHandler(
            IAddressRepository addressRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<object>> Handle(
            SetDefaultAddressCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<object>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            var address = await _addressRepository.GetByIdAsync(request.Id);
            if (address == null)
            {
                return result.BuildNotFound();
            }

            if (address.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _addressRepository.UnsetAllDefaultAddressesAsync(userId.Value);
                
                address.IsDefault = true;
                _addressRepository.Update(address);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction);

                return result.BuildSuccess("Default address set successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Failed to set default address: {ex.Message}");
            }
        }
    }
}
