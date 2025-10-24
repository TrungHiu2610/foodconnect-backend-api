using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class RejectShopRegistrationCommandHandler : IRequestHandler<RejectShopRegistrationCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RejectShopRegistrationCommandHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(RejectShopRegistrationCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();
            var response = new CreateOrUpdateResponse();

            var adminUserId = _currentUserService.UserId;
            if (adminUserId == null)
            {
                return result.BuildFail("Admin user not found");
            }

            // Lấy đơn đăng ký
            var registration = await _shopRegistrationRepository.GetByIdAsync(request.ShopRegistrationId);

            if (registration == null)
            {
                return result.BuildFail("Shop registration not found");
            }

            if (registration.Status != ShopRegistrationStatusEnum.Pending)
            {
                return result.BuildFail("Only pending shop registrations can be rejected");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Cập nhật status registration
                registration.Status = ShopRegistrationStatusEnum.Rejected;
                registration.ReviewedBy = adminUserId;
                registration.AdminReason = request.Reason;
                registration.UpdatedAtUtc = DateTime.UtcNow;
                _shopRegistrationRepository.Update(registration);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = registration.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Shop registration rejected successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Reject shop registration failed: {ex.Message}");
            }
        }
    }
}
