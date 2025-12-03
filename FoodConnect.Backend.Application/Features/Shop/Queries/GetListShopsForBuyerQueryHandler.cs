using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetListShopsForBuyerQueryHandler : IRequestHandler<GetListShopsForBuyerQuery, BaseResponse<PaginatedList<ShopListForBuyerResponse>>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public GetListShopsForBuyerQueryHandler(IShopRepository shopRepository, IMapper mapper)
        {
            _shopRepository = shopRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PaginatedList<ShopListForBuyerResponse>>> Handle(
            GetListShopsForBuyerQuery request, 
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<ShopListForBuyerResponse>>();

            var query = _shopRepository.GetShopsAsQueryable()
                .Include(s => s.ShopCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.OperatingHours)
                .Include(s => s.Products)
                .Where(s => s.Status == ShopStatusEnum.Active)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.TextSearch))
            {
                var searchLower = request.TextSearch.ToLower();
                query = query.Where(s =>
                    (s.ShopName != null && s.ShopName.ToLower().Contains(searchLower)) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchLower)) ||
                    (s.Street != null && s.Street.ToLower().Contains(searchLower)) ||
                    (s.Ward != null && s.Ward.ToLower().Contains(searchLower)) ||
                    (s.District != null && s.District.ToLower().Contains(searchLower)) ||
                    (s.City != null && s.City.ToLower().Contains(searchLower))
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

            if (request.HasPromotion.HasValue && request.HasPromotion.Value)
            {
                query = query.Where(s => s.IsPromoted);
            }

            var shops = await query.ToListAsync(cancellationToken);

            if (request.UserLatitude.HasValue && request.UserLongitude.HasValue)
            {
                foreach (var shop in shops)
                {
                    if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                    {
                        shop.CalculatedDistance = CalculateDistance(
                            request.UserLatitude.Value,
                            request.UserLongitude.Value,
                            shop.Latitude.Value,
                            shop.Longitude.Value
                        );
                    }
                }

                if (request.MaxDistance.HasValue)
                {
                    shops = shops.Where(s => s.CalculatedDistance.HasValue && s.CalculatedDistance.Value <= request.MaxDistance.Value).ToList();
                }
            }

            if (request.IsOpen.HasValue && request.IsOpen.Value)
            {
                shops = shops.Where(s => s.IsOpenNow()).ToList();
            }

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

            return result.BuildSuccess(paginatedList, "Get shops for buyer successfully");
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
                "newest" => isDescending
                    ? shops.OrderByDescending(s => s.CreatedAtUtc).ToList()
                    : shops.OrderBy(s => s.CreatedAtUtc).ToList(),
                "featured" => shops.OrderByDescending(s => s.IsFeatured)
                    .ThenByDescending(s => s.Rating ?? 0)
                    .ToList(),
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
            if (shop.IsPromoted) badges.Add("Giảm giá");
            if (shop.CreatedAtUtc >= DateTime.UtcNow.AddDays(-30)) badges.Add("Mới");
            if (shop.IsVerified) badges.Add("Đã xác minh");

            return badges;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return Math.Round(distance, 2);
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
