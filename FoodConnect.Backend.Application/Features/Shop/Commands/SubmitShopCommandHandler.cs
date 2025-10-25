using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class SubmitShopCommandHandler : IRequestHandler<SubmitShopCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SubmitShopCommandHandler(
            IShopRepository shopRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(SubmitShopCommand request, CancellationToken cancellationToken)
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

            // Check ownership
            if (shop.UserId != userId)
            {
                return result.BuildForbidden("You do not have permission to submit this shop");
            }

            // Validate status
            if (shop.Status != ShopStatusEnum.Draft)
            {
                return result.BuildFail($"Shop must be in Draft status to submit. Current status: {shop.Status}");
            }

            // Validate required assets
            var hasIdCardFront = shop.Assets.Any(a => a.AssetType == ShopAssetTypeEnum.IdCardFront);
            var hasIdCardBack = shop.Assets.Any(a => a.AssetType == ShopAssetTypeEnum.IdCardBack);
            var hasPortrait = shop.Assets.Any(a => a.AssetType == ShopAssetTypeEnum.PortraitPhoto);

            if (!hasIdCardFront || !hasIdCardBack || !hasPortrait)
            {
                return result.BuildFail("Missing required documents: ID card front, ID card back, and portrait photo are required");
            }

            // Change status to PendingApproval
            shop.Status = ShopStatusEnum.PendingApproval;
            shop.UpdatedAtUtc = DateTime.UtcNow;

            _shopRepository.Update(shop);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new CreateOrUpdateResponse
            {
                Id = shop.Id,
                IsSuccess = true
            }, "Shop submitted for approval successfully");
        }
    }
}
