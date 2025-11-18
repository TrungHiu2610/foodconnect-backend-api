using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Commands
{
    public class SubmitPromotionForApprovalCommandHandler : IRequestHandler<SubmitPromotionForApprovalCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SubmitPromotionForApprovalCommandHandler(
            IPromotionRepository promotionRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _promotionRepository = promotionRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(SubmitPromotionForApprovalCommand request, CancellationToken cancellationToken)
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

            if (promotion.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            if (promotion.Status != PromotionStatusEnum.Draft)
            {
                return result.BuildFail("Only draft promotions can be submitted for approval");
            }

            promotion.Status = PromotionStatusEnum.PendingApproval;
            _promotionRepository.Update(promotion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new CreateOrUpdateResponse { Id = promotion.Id }, "Promotion submitted for approval");
        }
    }
}
