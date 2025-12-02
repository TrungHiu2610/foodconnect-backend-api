using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetApplicablePromotionsQueryHandler : IRequestHandler<GetApplicablePromotionsQuery, BaseResponse<List<ApplicablePromotionResponse>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetApplicablePromotionsQueryHandler(
            IPromotionRepository promotionRepository,
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<ApplicablePromotionResponse>>> Handle(GetApplicablePromotionsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ApplicablePromotionResponse>>();
            var userId = _currentUserService.UserId;

            var promotions = await _promotionRepository.GetActivePromotionsByShopAsync(request.ShopId);
            var applicablePromotions = new List<ApplicablePromotionResponse>();

            foreach (var promotion in promotions)
            {
                var canApply = true;
                string? reasonCannotApply = null;

                if (request.OrderValue < promotion.MinimumOrderValue)
                {
                    canApply = false;
                    reasonCannotApply = $"Order value must be at least {promotion.MinimumOrderValue}";
                }

                if (canApply && !promotion.ApplicableToAllProducts && request.ProductIds != null && request.ProductIds.Any())
                {
                    var promotionProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
                    
                    var isAnyProductApplicable = request.ProductIds.Any(pid => promotionProductIds.Contains(pid));
                    if (!isAnyProductApplicable)
                    {
                        canApply = false;
                        // return list product name that are not in promotion
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (var pid in request.ProductIds)
                        {
                            if (!promotionProductIds.Contains(pid))
                            {
                                var productName = promotion.PromotionProducts.FirstOrDefault(pp => pp.ProductId == pid)?.Product?.Name;
                                stringBuilder.Append($"Product: {productName} is not applicable for this promotion. \n");
                            }
                        }
                        reasonCannotApply = stringBuilder.ToString();
                    }
                }

                if (canApply && promotion.MaxUsageCount.HasValue && promotion.TotalUsedCount >= promotion.MaxUsageCount.Value)
                {
                    canApply = false;
                    reasonCannotApply = "Promotion usage limit has been reached";
                }

                if (canApply && userId.HasValue)
                {
                    // Count how many times user has used this promotion via orders
                    var userOrders = await _orderRepository.GetOrdersByBuyerAsync(userId.Value, null);
                    var userUsageCount = userOrders.Count(o => o.PromotionId == promotion.Id);
                    
                    if (userUsageCount >= promotion.UsagePerCustomer)
                    {
                        canApply = false;
                        reasonCannotApply = "You have reached the usage limit for this promotion";
                    }
                }

                var discountAmount = promotion.PromotionType == PromotionTypeEnum.Percentage
                    ? request.OrderValue * promotion.DiscountValue / 100
                    : promotion.DiscountValue;

                var finalAmount = request.OrderValue - discountAmount;

                applicablePromotions.Add(new ApplicablePromotionResponse
                {
                    PromotionId = promotion.Id,
                    PromotionName = promotion.PromotionName,
                    PromotionType = (int)promotion.PromotionType,
                    DiscountValue = promotion.DiscountValue,
                    MinimumOrderValue = promotion.MinimumOrderValue,
                    OrderValue = request.OrderValue,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    CanApply = canApply,
                    ReasonCannotApply = reasonCannotApply
                });
            }

            return result.BuildSuccess(applicablePromotions, "Success");
        }
    }
}
