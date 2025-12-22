using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Features.Promotion.Services;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class ApprovePromotionCommandHandler : IRequestHandler<ApprovePromotionCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly PromotionNotificationService _promotionNotificationService;

        public ApprovePromotionCommandHandler(
            IPromotionRepository promotionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            PromotionNotificationService promotionNotificationService)
        {
            _promotionRepository = promotionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _promotionNotificationService = promotionNotificationService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ApprovePromotionCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

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

            if (promotion.Status != PromotionStatusEnum.PendingApproval)
            {
                return result.BuildFail("Only pending promotions can be approved");
            }

            var now = DateTime.UtcNow;
            if (promotion.StartDate <= now)
            {
                promotion.Status = PromotionStatusEnum.Active;
            }
            else
            {
                promotion.Status = PromotionStatusEnum.Approved;
            }

            promotion.ReviewedBy = userId.Value;
            promotion.ReviewedAt = now;

            _promotionRepository.Update(promotion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (promotion.Status == PromotionStatusEnum.Active)
            {
                await _promotionNotificationService.NotifyPromotionActivatedAsync(promotion, cancellationToken);
            }

            return result.BuildSuccess(new CreateOrUpdateResponse { Id = promotion.Id }, "Promotion approved successfully");
        }
    }
}
