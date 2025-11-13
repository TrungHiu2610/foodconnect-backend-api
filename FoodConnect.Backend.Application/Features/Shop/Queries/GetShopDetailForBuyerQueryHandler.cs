using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetShopDetailForBuyerQueryHandler : IRequestHandler<GetShopDetailForBuyerQuery, BaseResponse<ShopDetailForBuyerResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IProductRepository _productRepository;

        public GetShopDetailForBuyerQueryHandler(
            IShopRepository shopRepository,
            IProductRepository productRepository)
        {
            _shopRepository = shopRepository;
            _productRepository = productRepository;
        }

        public async Task<BaseResponse<ShopDetailForBuyerResponse>> Handle(
            GetShopDetailForBuyerQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ShopDetailForBuyerResponse>();

            try
            {
                // Get shop details with related data
                var shop = await _shopRepository.GetShopsAsQueryable()
                    .Where(s => s.Id == request.ShopId && s.Status == ShopStatusEnum.Active)
                    .Include(s => s.OperatingHours)
                    .Include(s => s.Assets)
                    .Include(s => s.ShopCategories)
                        .ThenInclude(sc => sc.Category)
                    .FirstOrDefaultAsync(cancellationToken);

                if (shop == null)
                {
                    return result.BuildNotFound();
                }

                // Calculate distance if user location provided
                double? distance = null;
                if (request.UserLatitude.HasValue && request.UserLongitude.HasValue &&
                    shop.Latitude.HasValue && shop.Longitude.HasValue)
                {
                    distance = CalculateDistance(
                        request.UserLatitude.Value,
                        request.UserLongitude.Value,
                        shop.Latitude.Value,
                        shop.Longitude.Value);
                }

                // Get available products (only Active products for buyer)
                var products = await _productRepository.GetProductsAsQueryable()
                    .Where(p => p.ShopId == shop.Id && p.Status == ProductStatusEnum.Active)
                    .Include(p => p.Category)
                    .Include(p => p.ProductAssets)
                    .ToListAsync(cancellationToken);

                // Map to response
                var response = new ShopDetailForBuyerResponse
                {
                    Id = shop.Id,
                    ShopName = shop.ShopName,
                    Description = shop.Description,
                    LogoUrl = shop.LogoUrl,
                    CoverImageUrl = shop.CoverImageUrl,
                    
                    // Location
                    Street = shop.Street ?? "",
                    Ward = shop.Ward ?? "",
                    District = shop.District ?? "",
                    City = shop.City ?? "",
                    Latitude = shop.Latitude,
                    Longitude = shop.Longitude,
                    Distance = distance,
                    
                    // Contact
                    PhoneNumber = shop.SellerPhone ?? "",
                    
                    // Stats
                    Rating = shop.Rating,
                    ReviewCount = shop.ReviewCount,
                    TotalOrders = shop.TotalOrders,
                    
                    // Status
                    IsOpen = shop.IsOpenNow(),
                    IsFeatured = shop.IsFeatured,
                    IsVerified = shop.IsVerified,
                    Badges = CalculateBadges(shop),
                    
                    // Categories
                    Categories = shop.ShopCategories?
                        .Select(sc => sc.Category?.Name ?? "")
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList() ?? new List<string>(),
                    
                    // Operating Hours
                    OperatingHours = shop.OperatingHours?
                        .OrderBy(oh => oh.DayOfWeek)
                        .Select(oh => new OperatingHourDto
                        {
                            DayOfWeek = oh.DayOfWeek.ToString(),
                            OpenTime = oh.OpenTime.ToString("HH:mm"),
                            CloseTime = oh.CloseTime.ToString("HH:mm"),
                            IsOpen = true // ShopOperatingHour doesn't have IsOpen field, assume open if record exists
                        })
                        .ToList() ?? new List<OperatingHourDto>(),
                    
                    // Additional Images (KitchenPhoto and FoodSafetyCertificate)
                    AdditionalImages = shop.Assets?
                        .Where(a => a.AssetType == ShopAssetTypeEnum.KitchenPhoto || a.AssetType == ShopAssetTypeEnum.FoodSafetyCertificate)
                        .Select(a => a.AssetUrl)
                        .ToList() ?? new List<string>(),
                    
                    // Available Products
                    AvailableProducts = products.Select(p => 
                    {
                        var dto = new ShopProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Price = p.Price,
                            DiscountPrice = null,
                            Stock = p.StockQuantity ?? 0,
                            IsAvailable = p.IsAvailable,
                            ThumbnailUrl = p.ProductAssets?
                                .FirstOrDefault(a => a.AssetType == ProductAssetTypeEnum.Image)?
                                .AssetUrl,
                            CategoryName = p.Category?.Name ?? "",
                            DeliveryType = p.Category?.DeliveryType.ToString() ?? "Standard",
                            ProductBadges = new List<string>()
                        };

                        // Check if Express product is outside delivery range
                        if (p.Category?.DeliveryType == DeliveryTypeEnum.Express)
                        {
                            if (distance.HasValue && distance.Value > (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                            {
                                dto.IsDeliverable = false;
                                dto.DeliverabilityMessage = $"Giao hàng Express chỉ khả dụng trong bán kính {ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km";
                                dto.ProductBadges.Add("Ngoài vùng giao hàng");
                            }
                            else if (!distance.HasValue)
                            {
                                dto.DeliverabilityMessage = "Bật vị trí để kiểm tra khả năng giao hàng Express";
                                dto.ProductBadges.Add("Cần xác nhận vị trí");
                            }
                            else
                            {
                                dto.IsDeliverable = true;
                            }
                        }
                        else // Standard delivery
                        {
                            dto.IsDeliverable = true;
                        }

                        return dto;
                    }).ToList()
                };

                return result.BuildSuccess(response, "Shop details retrieved successfully");
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to retrieve shop details: {ex.Message}");
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;

        private List<string> CalculateBadges(Domain.Entities.Shop shop)
        {
            var badges = new List<string>();

            if (shop.IsFeatured)
                badges.Add("Nổi bật");

            if (shop.Rating.HasValue && shop.Rating.Value >= 4.5m && shop.ReviewCount >= 100)
                badges.Add("Top rated");

            if (shop.TotalOrders >= 500)
                badges.Add("Bán chạy");

            if (shop.IsPromoted)
                badges.Add("Giảm giá");

            if (shop.CreatedAtUtc >= DateTime.UtcNow.AddMonths(-3))
                badges.Add("Mới");

            if (shop.IsVerified)
                badges.Add("Đã xác minh");

            return badges;
        }
    }
}
