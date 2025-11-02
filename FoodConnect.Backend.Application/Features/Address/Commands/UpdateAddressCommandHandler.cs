using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Address.Commands
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateAddressCommandHandler(
            IAddressRepository addressRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(
            UpdateAddressCommand request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

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
                if (request.IsDefault.HasValue && request.IsDefault.Value && !address.IsDefault)
                {
                    await _addressRepository.UnsetAllDefaultAddressesAsync(userId.Value);
                }

                _mapper.Map(request, address);

                _addressRepository.Update(address);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(transaction);

                return result.BuildSuccess(
                    new CreateOrUpdateResponse { Id = address.Id },
                    "Address updated successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Failed to update address: {ex.Message}");
            }
        }
    }
}
