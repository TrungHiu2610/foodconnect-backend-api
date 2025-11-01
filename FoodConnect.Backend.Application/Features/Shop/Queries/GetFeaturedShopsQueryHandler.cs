using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetFeaturedShopsQueryHandler : IRequestHandler<GetFeaturedShopsQuery, BaseResponse<List<ShopListForBuyerResponse>>>
    {
        private readonly IShopRepository _shopRepository;

        public GetFeaturedShopsQueryHandler(IShopRepository shopRepository)
        {
            _shopRepository = shopRepository;
        }

        public async Task<BaseResponse<List<ShopListForBuyerResponse>>> Handle(
            GetFeaturedShopsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ShopListForBuyerResponse>>();

            try
            {
                // Get featured and active shops
                var query = _shopRepository.GetShopsAsQueryable()
                    .Where(s => s.IsFeatured && s.Status == ShopStatusEnum.Active);

                // Calculate distance if user location provided
                if (request.UserLatitude.HasValue && request.UserLongitude.HasValue)
                {
                    var shops = await query.ToListAsync(cancellationToken);

                    // Calculate distance for each shop
                    foreach (var shop in shops)
                    {
                        if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                        {
                            shop.CalculatedDistance = CalculateDistance(
                                request.UserLatitude.Value,
                                request.UserLongitude.Value,
                                shop.Latitude.Value,
                                shop.Longitude.Value);
                        }
                    }

                    // Sort by Rating DESC, then TotalOrders DESC
                    shops = shops
                        .OrderByDescending(s => s.Rating)
                        .ThenByDescending(s => s.TotalOrders)
                        .Take(request.Limit)
                        .ToList();

                    var response = MapToResponse(shops);
                    return result.BuildSuccess(response, "Featured shops retrieved successfully");
                }
                else
                {
                    // No location provided - just sort and limit
                    var shops = await query
                        .OrderByDescending(s => s.Rating)
                        .ThenByDescending(s => s.TotalOrders)
                        .Take(request.Limit)
                        .ToListAsync(cancellationToken);

                    var response = MapToResponse(shops);
                    return result.BuildSuccess(response, "Featured shops retrieved successfully");
                }
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to retrieve featured shops: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula
        /// </summary>
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

        /// <summary>
        /// Map shop entities to response DTOs
        /// </summary>
        private List<ShopListForBuyerResponse> MapToResponse(List<Domain.Entities.Shop> shops)
        {
            return shops.Select(shop => new ShopListForBuyerResponse
            {
                Id = shop.Id,
                ShopName = shop.ShopName,
                LogoUrl = shop.LogoUrl,
                CoverImageUrl = shop.CoverImageUrl,
                MainCategories = shop.ShopCategories?
                    .OrderBy(sc => sc.Category?.Name)
                    .Take(3)
                    .Select(sc => sc.Category?.Name ?? "")
                    .ToList() ?? new List<string>(),
                Rating = shop.Rating,
                ReviewCount = shop.ReviewCount,
                TotalOrders = shop.TotalOrders,
                Distance = shop.CalculatedDistance,
                IsOpen = shop.IsOpenNow(),
                IsFeatured = shop.IsFeatured,
                IsVerified = shop.IsVerified,
                Badges = CalculateBadges(shop),
                Address = shop.GetFullAddress()
            }).ToList();
        }

        /// <summary>
        /// Calculate shop badges based on properties
        /// </summary>
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
