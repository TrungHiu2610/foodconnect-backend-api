using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class ValidatePromotionQueryHandler : IRequestHandler<ValidatePromotionQuery, BaseResponse<PromotionValidationResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public ValidatePromotionQueryHandler(
            IPromotionRepository promotionRepository,
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<PromotionValidationResponse>> Handle(ValidatePromotionQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PromotionValidationResponse>();
            var userId = _currentUserService.UserId;

            var promotion = await _promotionRepository.GetPromotionWithProductsAsync(request.PromotionId);
            if (promotion == null)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Promotion not found"
                }, "Validation completed");
            }

            if (promotion.ShopId != request.ShopId)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Promotion does not belong to this shop"
                }, "Validation completed");
            }

            if (promotion.Status != PromotionStatusEnum.Active)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Promotion is not active"
                }, "Validation completed");
            }

            if (promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Promotion is not within valid date range"
                }, "Validation completed");
            }

            if (request.OrderValue < promotion.MinimumOrderValue)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Order value must be at least {promotion.MinimumOrderValue}"
                }, "Validation completed");
            }

            if (!promotion.ApplicableToAllProducts && request.ProductIds != null && request.ProductIds.Any())
            {
                var promotionProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
                if (!request.ProductIds.Any(pid => promotionProductIds.Contains(pid)))
                {
                    return result.BuildSuccess(new PromotionValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "Promotion is not applicable to the selected products"
                    }, "Validation completed");
                }
            }

            if (promotion.MaxUsageCount.HasValue && promotion.TotalUsedCount >= promotion.MaxUsageCount.Value)
            {
                return result.BuildSuccess(new PromotionValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = "Promotion usage limit has been reached"
                }, "Validation completed");
            }

            if (userId.HasValue)
            {
                var userOrders = await _orderRepository.GetOrdersByBuyerAsync(userId.Value, null);
                var userUsageCount = userOrders.Count(o => o.PromotionId == promotion.Id);
                
                if (userUsageCount >= promotion.UsagePerCustomer)
                {
                    return result.BuildSuccess(new PromotionValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "You have reached the usage limit for this promotion"
                    }, "Validation completed");
                }
            }

            var discountAmount = promotion.PromotionType == PromotionTypeEnum.Percentage
                ? request.OrderValue * promotion.DiscountValue / 100
                : promotion.DiscountValue;

            return result.BuildSuccess(new PromotionValidationResponse
            {
                IsValid = true,
                DiscountAmount = discountAmount
            }, "Validation completed");
        }
    }
}
