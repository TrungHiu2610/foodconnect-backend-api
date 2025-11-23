using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetNewUserStatisticsQueryHandler : IRequestHandler<GetNewUserStatisticsQuery, BaseResponse<NewUserStatisticsResponse>>
    {
        private readonly IUserRepository _userRepository;

        public GetNewUserStatisticsQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<NewUserStatisticsResponse>> Handle(
            GetNewUserStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<NewUserStatisticsResponse>();

            var currentUsers = await _userRepository.GetAllQueryable()
                .Where(u => u.CreatedAtUtc >= request.FromDate &&
                           u.CreatedAtUtc <= request.ToDate)
                .ToListAsync(cancellationToken);

            var periodDays = (request.ToDate - request.FromDate).Days;
            var previousFromDate = request.FromDate.AddDays(-periodDays);
            var previousToDate = request.FromDate.AddDays(-1);

            var previousUsers = await _userRepository.GetAllQueryable()
                .Where(u => u.CreatedAtUtc >= previousFromDate &&
                           u.CreatedAtUtc <= previousToDate)
                .CountAsync(cancellationToken);

            var newUsersCount = currentUsers.Count;
            var growthRate = previousUsers > 0
                ? Math.Round(((decimal)(newUsersCount - previousUsers) / previousUsers) * 100, 2)
                : (newUsersCount > 0 ? 100 : 0);

            var timeSeries = new List<UserGrowthTimeSeries>();
            var cumulativeCount = 0;

            if (request.GroupBy.ToLower() == "day")
            {
                timeSeries = currentUsers
                    .GroupBy(u => u.CreatedAtUtc.Date)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        cumulativeCount += g.Count();
                        return new UserGrowthTimeSeries
                        {
                            Date = g.Key,
                            UserCount = g.Count(),
                            CumulativeCount = cumulativeCount
                        };
                    })
                    .ToList();
            }
            else if (request.GroupBy.ToLower() == "month")
            {
                timeSeries = currentUsers
                    .GroupBy(u => new DateTime(u.CreatedAtUtc.Year, u.CreatedAtUtc.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        cumulativeCount += g.Count();
                        return new UserGrowthTimeSeries
                        {
                            Date = g.Key,
                            UserCount = g.Count(),
                            CumulativeCount = cumulativeCount
                        };
                    })
                    .ToList();
            }
            else if (request.GroupBy.ToLower() == "year")
            {
                timeSeries = currentUsers
                    .GroupBy(u => new DateTime(u.CreatedAtUtc.Year, 1, 1))
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        cumulativeCount += g.Count();
                        return new UserGrowthTimeSeries
                        {
                            Date = g.Key,
                            UserCount = g.Count(),
                            CumulativeCount = cumulativeCount
                        };
                    })
                    .ToList();
            }

            var response = new NewUserStatisticsResponse
            {
                NewUsersCount = newUsersCount,
                GrowthRate = growthRate,
                PreviousPeriodCount = previousUsers,
                TimeSeries = timeSeries,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            return result.BuildSuccess(response, "New user statistics retrieved successfully");
        }
    }

    public class GetTopSellersQueryHandler : IRequestHandler<GetTopSellersQuery, BaseResponse<TopSellersResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IOrderRepository _orderRepository;

        public GetTopSellersQueryHandler(
            IShopRepository shopRepository,
            IOrderRepository orderRepository)
        {
            _shopRepository = shopRepository;
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<TopSellersResponse>> Handle(
            GetTopSellersQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<TopSellersResponse>();

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                    .ThenInclude(s => s.User)
                .Where(o => o.Status == OrderStatusEnum.Completed);

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= request.ToDate.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var sellerStats = orders
                .GroupBy(o => new
                {
                    o.ShopId,
                    o.Shop.ShopName,
                    SellerId = o.Shop.UserId,
                    SellerName = o.Shop.User.FullName,
                    o.Shop.Rating,
                    o.Shop.ReviewCount
                })
                .Select(g => new TopSellerItem
                {
                    SellerId = g.Key.SellerId,
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    SellerName = g.Key.SellerName,
                    Rating = g.Key.Rating,
                    TotalRevenue = g.Sum(o => (decimal)o.Total),
                    TotalOrders = g.Count(),
                    ReviewCount = g.Key.ReviewCount
                })
                .ToList();

            var topSellersByRevenue = sellerStats
                .OrderByDescending(s => s.TotalRevenue)
                .Take(request.TopN)
                .ToList();

            var topSellersByOrders = sellerStats
                .OrderByDescending(s => s.TotalOrders)
                .Take(request.TopN)
                .ToList();

            var topSellersByRating = sellerStats
                .Where(s => s.Rating.HasValue && s.ReviewCount >= 5)
                .OrderByDescending(s => s.Rating)
                .ThenByDescending(s => s.ReviewCount)
                .Take(request.TopN)
                .ToList();

            var response = new TopSellersResponse
            {
                TopSellersByRevenue = topSellersByRevenue,
                TopSellersByOrders = topSellersByOrders,
                TopSellersByRating = topSellersByRating
            };

            return result.BuildSuccess(response, "Top sellers retrieved successfully");
        }
    }

    public class GetLoyalCustomersQueryHandler : IRequestHandler<GetLoyalCustomersQuery, BaseResponse<LoyalCustomersResponse>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetLoyalCustomersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<LoyalCustomersResponse>> Handle(
            GetLoyalCustomersQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<LoyalCustomersResponse>();

            var orders = await _orderRepository.GetAllQueryable()
                .Include(o => o.Buyer)
                .Where(o => o.Status == OrderStatusEnum.Completed)
                .ToListAsync(cancellationToken);

            var customerStats = orders
                .GroupBy(o => new
                {
                    o.BuyerId,
                    BuyerName = o.Buyer.FullName,
                    o.Buyer.Email
                })
                .Select(g => new LoyalCustomerItem
                {
                    BuyerId = g.Key.BuyerId,
                    BuyerName = g.Key.BuyerName,
                    Email = g.Key.Email,
                    TotalOrders = g.Count(),
                    TotalSpending = g.Sum(o => (decimal)o.Total),
                    FirstOrderDate = g.Min(o => o.CreatedAtUtc),
                    LastOrderDate = g.Max(o => o.CreatedAtUtc)
                })
                .ToList();

            var topCustomersByOrders = customerStats
                .OrderByDescending(c => c.TotalOrders)
                .Take(request.TopN)
                .ToList();

            var topCustomersBySpending = customerStats
                .OrderByDescending(c => c.TotalSpending)
                .Take(request.TopN)
                .ToList();

            var response = new LoyalCustomersResponse
            {
                TopCustomersByOrders = topCustomersByOrders,
                TopCustomersBySpending = topCustomersBySpending
            };

            return result.BuildSuccess(response, "Loyal customers retrieved successfully");
        }
    }
}
