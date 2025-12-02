using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class CancelPromotionCommandHandler : IRequestHandler<CancelPromotionCommand, BaseResponse<DeleteResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CancelPromotionCommandHandler(
            IPromotionRepository promotionRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<DeleteResponse>> Handle(CancelPromotionCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<DeleteResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var promotion = await _promotionRepository.GetDetailByIdAsync(request.PromotionId);
            if (promotion == null)
            {
                return result.BuildNotFound("Promotion not found");
            }

            if (promotion.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            if (promotion.Status == PromotionStatusEnum.Cancelled || promotion.Status == PromotionStatusEnum.Expired)
            {
                return result.BuildFail("Promotion is already cancelled or expired");
            }

            if (promotion.Status == PromotionStatusEnum.Active)
            {
                // Check if promotion has been used in any orders
                var orders = await _orderRepository.GetOrdersByShopAsync(promotion.ShopId, null);
                var hasBeenUsed = orders.Any(o => o.PromotionId == request.PromotionId);
                
                if (hasBeenUsed)
                {
                    return result.BuildFail("Cannot cancel promotion that has been used");
                }
            }

            promotion.Status = PromotionStatusEnum.Cancelled;
            _promotionRepository.Update(promotion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new DeleteResponse { DeletedCount = 1 }, "Promotion cancelled successfully");
        }
    }
}
