using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductReviewRepository _reviewRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderDetailQueryHandler(
            IOrderRepository orderRepository,
            IProductReviewRepository reviewRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _reviewRepository = reviewRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Check if user has permission to view this order
            var userId = _currentUserService.UserId.Value;
            var isBuyer = order.BuyerId == userId;
            var isSeller = order.Shop?.UserId == userId;
            var isAdmin = _currentUserService.Role == "Admin";

            if (!isBuyer && !isSeller && !isAdmin)
            {
                return result.BuildForbidden("You don't have permission to view this order");
            }

            var orderDto = OrderMapper.MapToDetailDto(order);
            
            // Calculate review status and mark reviewed items (only for buyers and completed orders)
            if (isBuyer && order.Status == OrderStatusEnum.Completed)
            {
                var reviews = await _reviewRepository.GetAllAsync();
                var orderReviews = reviews
                    .Where(r => r.OrderId == request.OrderId && r.BuyerId == userId)
                    .ToList();
                
                // Mark which items have been reviewed
                foreach (var item in orderDto.OrderItems)
                {
                    item.IsReviewed = orderReviews.Any(r => r.ProductId == item.ProductId);
                }
                
                // Calculate overall review status
                var totalItems = orderDto.OrderItems.Count;
                var reviewedItems = orderDto.OrderItems.Count(i => i.IsReviewed);
                
                orderDto.ReviewStatus = reviewedItems >= totalItems 
                    ? OrderReviewStatusEnum.FullyReviewed 
                    : OrderReviewStatusEnum.NotReviewed;
            }

            return result.BuildSuccess(orderDto, "Order retrieved successfully");
        }
    }
}
