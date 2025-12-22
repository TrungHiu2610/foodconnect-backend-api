using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetShopsWithActivePromotionsQueryHandler : IRequestHandler<GetShopsWithActivePromotionsQuery, BaseResponse<PaginatedList<ShopListForBuyerResponse>>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IDistanceCalculatorService _distanceCalculator;

        public GetShopsWithActivePromotionsQueryHandler(
            IShopRepository shopRepository,
            IPromotionRepository promotionRepository,
            IDistanceCalculatorService distanceCalculator)
        {
            _shopRepository = shopRepository;
            _promotionRepository = promotionRepository;
            _distanceCalculator = distanceCalculator;
        }

        public async Task<BaseResponse<PaginatedList<ShopListForBuyerResponse>>> Handle(
            GetShopsWithActivePromotionsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<ShopListForBuyerResponse>>();

            var now = DateTime.UtcNow;

            // Get all shops with active promotions
            var activePromotions = await _promotionRepository.GetPromotionsByStatusAsync(PromotionStatusEnum.Active);
            
            // Filter promotions that are currently valid (within date range and usage limits)
            var validPromotions = activePromotions
                .Where(p => p.StartDate <= now && p.EndDate >= now)
                .Where(p => !p.MaxUsageCount.HasValue || p.TotalUsedCount < p.MaxUsageCount.Value)
                .ToList();
            
            var activePromotionShopIds = validPromotions
                .Select(p => p.ShopId)
                .Distinct()
                .ToList();

            if (!activePromotionShopIds.Any())
            {
                return result.BuildSuccess(
                    new PaginatedList<ShopListForBuyerResponse>(new List<ShopListForBuyerResponse>(), 0, 1, request.PageSize),
                    "No shops with active promotions found"
                );
            }

            var query = _shopRepository.GetShopsAsQueryable()
                .Include(s => s.ShopCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.OperatingHours)
                .Include(s => s.Products)
                .Where(s => s.Status == ShopStatusEnum.Active)
                .Where(s => activePromotionShopIds.Contains(s.Id))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.TextSearch))
            {
                var searchLower = request.TextSearch.ToLower();
                query = query.Where(s =>
                    (s.ShopName != null && s.ShopName.ToLower().Contains(searchLower)) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchLower))
                );
            }

            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                query = query.Where(s => s.ShopCategories.Any(sc => request.CategoryIds.Contains(sc.CategoryId)));
            }

            if (request.MinRating.HasValue)
            {
                query = query.Where(s => s.Rating >= request.MinRating.Value);
            }

            var shops = await query.ToListAsync(cancellationToken);

            // Calculate distance if user location is provided
            if (request.UserLatitude.HasValue && request.UserLongitude.HasValue)
            {
                foreach (var shop in shops)
                {
                    if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                    {
                        shop.CalculatedDistance = _distanceCalculator.CalculateDistance(
                            request.UserLatitude.Value,
                            request.UserLongitude.Value,
                            shop.Latitude.Value,
                            shop.Longitude.Value
                        );
                    }
                }

                // Filter by max distance if specified
                if (request.MaxDistanceKm.HasValue)
                {
                    shops = shops
                        .Where(s => s.CalculatedDistance.HasValue && s.CalculatedDistance.Value <= request.MaxDistanceKm.Value)
                        .ToList();
                }
            }

            // Sort
            shops = ApplySorting(shops, request.SortBy, request.IsDescending);

            var totalItems = shops.Count;
            var paginatedShops = shops
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var shopResponses = paginatedShops.Select(shop => new ShopListForBuyerResponse
            {
                Id = shop.Id,
                ShopName = shop.ShopName,
                LogoUrl = shop.LogoUrl,
                CoverImageUrl = shop.CoverImageUrl,
                MainCategories = shop.ShopCategories
                    .Take(3)
                    .Select(sc => sc.Category.Name)
                    .ToList(),
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

            var paginatedList = new PaginatedList<ShopListForBuyerResponse>(
                shopResponses,
                totalItems,
                request.PageNumber,
                request.PageSize
            );

            return result.BuildSuccess(paginatedList, $"Found {totalItems} shops with active promotions");
        }

        private List<Domain.Entities.Shop> ApplySorting(List<Domain.Entities.Shop> shops, string? sortBy, bool isDescending)
        {
            var sortedShops = sortBy?.ToLower() switch
            {
                "rating" => isDescending
                    ? shops.OrderByDescending(s => s.Rating ?? 0).ToList()
                    : shops.OrderBy(s => s.Rating ?? 0).ToList(),
                "distance" => isDescending
                    ? shops.OrderByDescending(s => s.CalculatedDistance ?? double.MaxValue).ToList()
                    : shops.OrderBy(s => s.CalculatedDistance ?? double.MaxValue).ToList(),
                "orders" => isDescending
                    ? shops.OrderByDescending(s => s.TotalOrders).ToList()
                    : shops.OrderBy(s => s.TotalOrders).ToList(),
                _ => shops.OrderByDescending(s => s.Rating ?? 0).ToList() // Default: sort by rating
            };

            return sortedShops;
        }

        private List<string> CalculateBadges(Domain.Entities.Shop shop)
        {
            var badges = new List<string>();

            if (shop.IsFeatured) badges.Add("Nổi bật");
            if (shop.Rating >= 4.5m) badges.Add("Top rated");
            if (shop.TotalOrders > 1000) badges.Add("Bán chạy");
            badges.Add("Giảm giá");
            if (shop.CreatedAtUtc >= DateTime.UtcNow.AddDays(-30)) badges.Add("Mới");
            if (shop.IsVerified) badges.Add("Đã xác minh");

            return badges;
        }
    }
}
