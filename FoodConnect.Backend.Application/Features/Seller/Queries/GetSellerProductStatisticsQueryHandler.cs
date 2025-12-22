using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerProductStatisticsQueryHandler : IRequestHandler<GetSellerProductStatisticsQuery, BaseResponse<SellerProductStatisticsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IRedisService _redisService;

        public GetSellerProductStatisticsQueryHandler(
            IOrderRepository orderRepository,
            IShopRepository shopRepository,
            IRedisService redisService)
        {
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<SellerProductStatisticsResponse>> Handle(
            GetSellerProductStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<SellerProductStatisticsResponse>();

            var cacheKey = CacheKeys.SellerProductStats(request.ShopId, request.FromDate, request.ToDate, request.TopN);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<SellerProductStatisticsResponse>>(cacheKey);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductAssets)
                .Where(o => o.ShopId == request.ShopId &&
                           o.Status == OrderStatusEnum.Completed);

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= request.ToDate.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var productSalesData = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Product = g.First().Product,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => (decimal)oi.TotalPrice)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(request.TopN)
                .ToList();

            var totalRevenue = productSalesData.Sum(x => x.TotalRevenue);

            var bestSellingProducts = productSalesData.Select(x => new ProductSalesItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                ProductImageUrl = x.Product?.ProductAssets?.FirstOrDefault()?.AssetUrl,
                QuantitySold = x.QuantitySold,
                TotalRevenue = x.TotalRevenue,
                ContributionPercentage = totalRevenue > 0
                    ? Math.Round((x.TotalRevenue / totalRevenue) * 100, 2)
                    : 0,
                AveragePrice = x.QuantitySold > 0
                    ? Math.Round(x.TotalRevenue / x.QuantitySold, 2)
                    : 0
            }).ToList();

            var response = new SellerProductStatisticsResponse
            {
                BestSellingProducts = bestSellingProducts,
                TotalRevenue = totalRevenue,
                TotalQuantitySold = productSalesData.Sum(x => x.QuantitySold)
            };

            var successResult = result.BuildSuccess(response, "Product statistics retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.SellerStats);

            return successResult;
        }
    }
}
