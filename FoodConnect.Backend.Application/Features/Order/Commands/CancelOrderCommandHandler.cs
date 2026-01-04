using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using Hangfire;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, BaseResponse<OrderDetailDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
        }

        public async Task<BaseResponse<OrderDetailDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OrderDetailDto>();

            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in");
            }

            var buyerId = _currentUserService.UserId.Value;

            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            if (order.BuyerId != buyerId)
            {
                return result.BuildForbidden("You don't have permission to cancel this order");
            }

            if (order.Status != OrderStatusEnum.Pending && order.Status != OrderStatusEnum.AwaitingPayment)
            {
                return result.BuildFail("Only pending or awaiting payment orders can be cancelled");
            }

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null && product.StockQuantity.HasValue)
                {
                    product.StockQuantity += orderItem.Quantity;
                    
                    if (product.StockQuantity > 0 && !product.IsAvailable)
                    {
                        product.IsAvailable = true;
                    }
                    
                    _productRepository.Update(product);
                }
            }

            order.Status = OrderStatusEnum.Cancelled;
            order.CancelReason = request.CancelReason;
            order.CancelledAt = DateTime.UtcNow;

            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            var orderDto = OrderMapper.MapToDetailDto(order!);

            await _orderNotificationService.NotifyOrderCancelledAsync(order!, true, cancellationToken);

            return result.BuildSuccess(orderDto, "Order cancelled successfully");
        }
    }
}
