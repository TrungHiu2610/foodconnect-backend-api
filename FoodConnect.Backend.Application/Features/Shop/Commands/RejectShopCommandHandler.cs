using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class RejectShopCommandHandler : IRequestHandler<RejectShopCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RejectShopCommandHandler(
            IShopRepository shopRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(RejectShopCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized("User not found");
            }

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            // Validate status
            if (shop.Status != ShopStatusEnum.PendingApproval)
            {
                return result.BuildFail($"Shop must be in PendingApproval status. Current status: {shop.Status}");
            }

            // Update shop status
            shop.Status = ShopStatusEnum.Rejected;
            shop.AdminReason = request.Reason;
            shop.ReviewedBy = userId;
            shop.ReviewedAt = DateTime.UtcNow;

            _shopRepository.Update(shop);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = shop.Id,
                IsSuccess = true
            }, "Shop rejected successfully");
        }
    }
}
