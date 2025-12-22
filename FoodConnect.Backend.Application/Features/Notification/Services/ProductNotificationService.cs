using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;
using ProductEntity = FoodConnect.Backend.Domain.Entities.Product;

namespace FoodConnect.Backend.Application.Features.Notification.Services
{
    public class ProductNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductNotificationService(
            INotificationService notificationService,
            INotificationRepository notificationRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task NotifyProductOutOfStockAsync(ProductEntity product, CancellationToken cancellationToken = default)
        {
            var buyersWithCart = await _cartRepository.GetBuyersWithProductInCartAsync(product.Id);

            var buyersWithPendingOrders = await _orderRepository.GetBuyersWithPendingOrdersContainingProductAsync(product.Id);

            var affectedBuyerIds = buyersWithCart
                .Union(buyersWithPendingOrders)
                .Distinct()
                .ToList();

            if (!affectedBuyerIds.Any())
                return;

            var metadata = new
            {
                productId = product.Id,
                productName = product.Name,
                shopId = product.ShopId,
                price = product.Price
            };

            foreach (var buyerId in affectedBuyerIds)
            {
                var notification = new Domain.Entities.Notification
                {
                    UserId = buyerId,
                    Type = NotificationTypeEnum.ProductOutOfStock,
                    Title = "⚠️ Sản phẩm đã hết hàng",
                    Message = $"Sản phẩm \"{product.Name}\" trong giỏ hàng/đơn hàng của bạn đã hết hàng",
                    OrderId = null,
                    ShopId = product.ShopId,
                    MetadataJson = JsonSerializer.Serialize(metadata),
                    IsRead = false
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var buyerId in affectedBuyerIds)
            {
                var notificationDto = new NotificationDto
                {
                    Type = NotificationTypeEnum.ProductOutOfStock,
                    Title = "⚠️ Sản phẩm đã hết hàng",
                    Message = $"Sản phẩm \"{product.Name}\" trong giỏ hàng/đơn hàng của bạn đã hết hàng",
                    ShopId = product.ShopId,
                    ShopName = product.Shop?.ShopName,
                    MetadataJson = JsonSerializer.Serialize(metadata),
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.SendToUserAsync(buyerId, notificationDto);

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(buyerId);
                await _notificationService.UpdateUnreadCountAsync(buyerId, unreadCount);
            }
        }
        public async Task NotifyProductBackInStockAsync(ProductEntity product, CancellationToken cancellationToken = default)
        {
            var buyersWithCart = await _cartRepository.GetBuyersWithProductInCartAsync(product.Id);
            var buyersWithPendingOrders = await _orderRepository.GetBuyersWithPendingOrdersContainingProductAsync(product.Id);
            
            var buyerIds = buyersWithCart
                .Union(buyersWithPendingOrders)
                .Distinct()
                .ToList();

            if (!buyerIds.Any())
                return;

            var metadata = new
            {
                productId = product.Id,
                productName = product.Name,
                shopId = product.ShopId,
                price = product.Price,
                stockQuantity = product.StockQuantity
            };

            foreach (var buyerId in buyerIds)
            {
                var notification = new Domain.Entities.Notification
                {
                    UserId = buyerId,
                    Type = NotificationTypeEnum.ProductBackInStock,
                    Title = "✅ Sản phẩm đã có hàng trở lại",
                    Message = $"Sản phẩm \"{product.Name}\" đã có hàng trở lại. Đặt ngay!",
                    OrderId = null,
                    ShopId = product.ShopId,
                    MetadataJson = JsonSerializer.Serialize(metadata),
                    IsRead = false
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var buyerId in buyerIds)
            {
                var notificationDto = new NotificationDto
                {
                    Type = NotificationTypeEnum.ProductBackInStock,
                    Title = "✅ Sản phẩm đã có hàng trở lại",
                    Message = $"Sản phẩm \"{product.Name}\" đã có hàng trở lại. Đặt ngay!",
                    ShopId = product.ShopId,
                    ShopName = product.Shop?.ShopName,
                    MetadataJson = JsonSerializer.Serialize(metadata),
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.SendToUserAsync(buyerId, notificationDto);

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(buyerId);
                await _notificationService.UpdateUnreadCountAsync(buyerId, unreadCount);
            }
        }
    }
}
