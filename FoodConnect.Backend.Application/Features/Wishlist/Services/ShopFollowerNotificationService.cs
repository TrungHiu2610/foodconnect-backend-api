using FoodConnect.Backend.Application.Commons.DTOs.Notifications;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using System.Text.Json;
using ProductEntity = FoodConnect.Backend.Domain.Entities.Product;

namespace FoodConnect.Backend.Application.Features.Wishlist.Services
{
    public class ShopFollowerNotificationService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public ShopFollowerNotificationService(
            IWishlistRepository wishlistRepository,
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task NotifyFollowersAboutPromotionAsync(Domain.Entities.Promotion promotion, CancellationToken cancellationToken = default)
        {
            if (promotion.Shop == null)
            {
                return;
            }

            // Get all users who follow this shop with notifications enabled
            var followers = await _wishlistRepository.GetShopFollowersWithNotificationsAsync(promotion.ShopId);

            if (!followers.Any())
            {
                return;
            }

            var discountText = promotion.PromotionType == PromotionTypeEnum.Percentage
                ? $"{promotion.DiscountValue}%"
                : $"{promotion.DiscountValue:N0} VNĐ";

            foreach (var wishlist in followers)
            {
                var notification = new Domain.Entities.Notification
                {
                    UserId = wishlist.UserId,
                    Type = NotificationTypeEnum.SystemAnnouncement,
                    Title = "Khuyến mãi mới từ shop yêu thích",
                    Message = $"{promotion.Shop.ShopName} vừa có khuyến mãi mới: \"{promotion.PromotionName}\" - Giảm {discountText}!",
                    ShopId = promotion.ShopId,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        PromotionId = promotion.Id,
                        PromotionName = promotion.PromotionName,
                        ShopName = promotion.Shop.ShopName,
                        DiscountValue = promotion.DiscountValue,
                        PromotionType = promotion.PromotionType.ToString(),
                        MinimumOrderValue = promotion.MinimumOrderValue,
                        StartDate = promotion.StartDate,
                        EndDate = promotion.EndDate
                    })
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notifications
            foreach (var wishlist in followers)
            {
                var notificationDto = new NotificationDto
                {
                    Type = NotificationTypeEnum.SystemAnnouncement,
                    Title = "Khuyến mãi mới từ shop yêu thích",
                    Message = $"{promotion.Shop.ShopName} vừa có khuyến mãi mới: \"{promotion.PromotionName}\" - Giảm {discountText}!",
                    ShopId = promotion.ShopId,
                    ShopName = promotion.Shop.ShopName,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        PromotionId = promotion.Id,
                        PromotionName = promotion.PromotionName,
                        ShopName = promotion.Shop.ShopName,
                        DiscountValue = promotion.DiscountValue,
                        PromotionType = promotion.PromotionType.ToString()
                    }),
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.SendToUserAsync(wishlist.UserId, notificationDto);

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(wishlist.UserId);
                await _notificationService.UpdateUnreadCountAsync(wishlist.UserId, unreadCount);
            }
        }

        public async Task NotifyFollowersAboutNewProductAsync(ProductEntity product, CancellationToken cancellationToken = default)
        {
            if (product.Shop == null)
            {
                return;
            }

            // Get all users who follow this shop with notifications enabled
            var followers = await _wishlistRepository.GetShopFollowersWithNotificationsAsync(product.ShopId);

            if (!followers.Any())
            {
                return;
            }

            var productImage = product.ProductAssets?.FirstOrDefault()?.AssetUrl;

            foreach (var wishlist in followers)
            {
                var notification = new Domain.Entities.Notification
                {
                    UserId = wishlist.UserId,
                    Type = NotificationTypeEnum.SystemAnnouncement,
                    Title = "Món mới từ shop yêu thích",
                    Message = $"{product.Shop.ShopName} vừa thêm món mới: \"{product.Name}\" - Giá {product.Price:N0} VNĐ",
                    ShopId = product.ShopId,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ShopName = product.Shop.ShopName,
                        Price = product.Price,
                        Description = product.Description,
                        ImageUrl = productImage,
                        CategoryId = product.CategoryId
                    })
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notifications
            foreach (var wishlist in followers)
            {
                var notificationDto = new NotificationDto
                {
                    Type = NotificationTypeEnum.SystemAnnouncement,
                    Title = "Món mới từ shop yêu thích",
                    Message = $"{product.Shop.ShopName} vừa thêm món mới: \"{product.Name}\" - Giá {product.Price:N0} VNĐ",
                    ShopId = product.ShopId,
                    ShopName = product.Shop.ShopName,
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        ImageUrl = productImage
                    }),
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.SendToUserAsync(wishlist.UserId, notificationDto);

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(wishlist.UserId);
                await _notificationService.UpdateUnreadCountAsync(wishlist.UserId, unreadCount);
            }
        }
        public async Task<int> GetFollowerCountAsync(Guid shopId)
        {
            var followers = await _wishlistRepository.GetShopFollowersWithNotificationsAsync(shopId);
            return followers.Count;
        }
    }
}
