using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, BaseResponse<object>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteAddressCommandHandler(
            IAddressRepository addressRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<object>> Handle(
            DeleteAddressCommand request,
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

            try
            {
                _addressRepository.Remove(address);
                await _unitOfWork.SaveChangesAsync();

                return result.BuildSuccess("Address deleted successfully");
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to delete address: {ex.Message}");
            }
        }
    }
}
