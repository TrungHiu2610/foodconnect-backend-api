using FoodConnect.Backend.Application.Features.Order.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Hangfire;

namespace FoodConnect.Backend.Application.Features.Order.Jobs
{
    public class OrderStatusJob
    {
        private readonly IOrderRepository _orderRepository;
        private readonly OrderAutoCompletionService _autoCompletionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderStatusJob> _logger;

        public OrderStatusJob(
            IOrderRepository orderRepository,
            OrderAutoCompletionService autoCompletionService,
            IUnitOfWork unitOfWork,
            ILogger<OrderStatusJob> logger)
        {
            _orderRepository = orderRepository;
            _autoCompletionService = autoCompletionService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 900)] // Prevent concurrent runs, 15 min timeout
        public async Task AutoCancelUnconfirmedOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Starting auto-cancel unconfirmed orders job at {Time}", DateTime.UtcNow);

                var now = DateTime.UtcNow;
                var expressTimeout = now.AddHours(-2);
                var standardTimeout = now.AddDays(-2);

                var allOrders = await _orderRepository.GetAllAsync();
                var pendingOrders = allOrders.Where(o => o.Status == OrderStatusEnum.Pending).ToList();

                var ordersToCancel = pendingOrders.Where(order =>
                {
                    if (order.DeliveryType == DeliveryTypeEnum.Express)
                    {
                        return order.CreatedAtUtc <= expressTimeout;
                    }
                    else // Standard
                    {
                        return order.CreatedAtUtc <= standardTimeout;
                    }
                }).ToList();

                _logger.LogInformation("Found {Count} orders to auto-cancel", ordersToCancel.Count);

                foreach (var order in ordersToCancel)
                {
                    try
                    {
                        order.Status = OrderStatusEnum.Cancelled;
                        order.CancelReason = "Đơn hàng tự động hủy do người bán không xác nhận trong thời gian quy định";
                        order.CancelledAt = DateTime.UtcNow;
                        
                        _orderRepository.Update(order);

                        _logger.LogInformation(
                            "Auto-cancelled order {OrderCode} (Type: {DeliveryType}, Created: {CreatedAt})",
                            order.OrderCode,
                            order.DeliveryType,
                            order.CreatedAtUtc);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cancelling order {OrderId}", order.Id);
                    }
                }

                if (ordersToCancel.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Successfully auto-cancelled {Count} orders", ordersToCancel.Count);
                }

                _logger.LogInformation("Completed auto-cancel unconfirmed orders job at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoCancelUnconfirmedOrdersAsync job");
            }
        }

        [DisableConcurrentExecution(timeoutInSeconds: 900)] // Prevent concurrent runs, 15 min timeout
        public async Task AutoCompleteDeliveredOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Starting auto-complete delivered orders job at {Time}", DateTime.UtcNow);

                var now = DateTime.UtcNow;
                var expressTimeout = now.AddHours(-2);
                var standardTimeout = now.AddDays(-2);

                var allOrders = await _orderRepository.GetAllAsync();
                var deliveredOrders = allOrders.Where(o => o.Status == OrderStatusEnum.Delivered).ToList();

                var ordersToComplete = deliveredOrders.Where(order =>
                {
                    if (!order.DeliveredAt.HasValue)
                        return false;

                    if (order.DeliveryType == DeliveryTypeEnum.Express)
                    {
                        return order.DeliveredAt.Value <= expressTimeout;
                    }
                    else // Standard
                    {
                        return order.DeliveredAt.Value <= standardTimeout;
                    }
                }).ToList();

                _logger.LogInformation("Found {Count} orders to auto-complete", ordersToComplete.Count);

                foreach (var order in ordersToComplete)
                {
                    try
                    {
                        var orderWithDetails = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
                        
                        if (orderWithDetails != null)
                        {
                            var success = await _autoCompletionService.CompleteOrderAsync(orderWithDetails);
                            
                            if (success)
                            {
                                _logger.LogInformation(
                                    "Successfully auto-completed order {OrderCode} (Type: {DeliveryType}, Delivered: {DeliveredAt})",
                                    orderWithDetails.OrderCode,
                                    orderWithDetails.DeliveryType,
                                    orderWithDetails.DeliveredAt);
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Failed to auto-complete order {OrderCode}",
                                    orderWithDetails.OrderCode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error completing order {OrderId}", order.Id);
                    }
                }

                _logger.LogInformation("Completed auto-complete delivered orders job at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoCompleteDeliveredOrdersAsync job");
            }
        }
    }
}
