using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByBuyerQueryHandler : IRequestHandler<GetOrdersByBuyerQuery, BaseResponse<PaginatedList<OrderSummaryDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductReviewRepository _reviewRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersByBuyerQueryHandler(
            IOrderRepository orderRepository,
            IProductReviewRepository reviewRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<PaginatedList<OrderSummaryDto>>> Handle(GetOrdersByBuyerQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<OrderSummaryDto>>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;
            var orders = await _orderRepository.GetOrdersByBuyerAsync(buyerId, request.Status);

            var orderDtos = orders.Select(o => OrderMapper.MapToSummaryDto(o)).ToList();
            
            foreach (var orderDto in orderDtos.Where(o => o.Status == OrderStatusEnum.Completed))
            {
                orderDto.ReviewStatus = await CalculateReviewStatusAsync(orderDto.Id, buyerId);
            }
            
            if (request.ReviewStatus.HasValue)
            {
                orderDtos = orderDtos.Where(o => o.ReviewStatus == request.ReviewStatus.Value).ToList();
            }

            var totalCount = orderDtos.Count;
            var paginatedOrders = orderDtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedList = new PaginatedList<OrderSummaryDto>(
                paginatedOrders,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            return result.BuildSuccess(paginatedList, "Orders retrieved successfully");
        }
        
        private async Task<OrderReviewStatusEnum> CalculateReviewStatusAsync(Guid orderId, Guid buyerId)
        {
            var reviews = await _reviewRepository.GetAllAsync();
            var orderReviews = reviews.Where(r => r.OrderId == orderId && r.BuyerId == buyerId).ToList();
            
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
            {
                return OrderReviewStatusEnum.NotReviewed;
            }
            
            var totalProducts = order.OrderItems.Count;
            var reviewedProducts = orderReviews.Count;
            
            return reviewedProducts >= totalProducts 
                ? OrderReviewStatusEnum.FullyReviewed 
                : OrderReviewStatusEnum.NotReviewed;
        }
    }
}
