using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetComplaintStatisticsQueryHandler : IRequestHandler<GetComplaintStatisticsQuery, BaseResponse<ComplaintStatisticsResponse>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetComplaintStatisticsQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<BaseResponse<ComplaintStatisticsResponse>> Handle(
            GetComplaintStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ComplaintStatisticsResponse>();

            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .AsQueryable();

            if (request.FromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= request.ToDate.Value);
            }

            var orders = await ordersQuery.ToListAsync(cancellationToken);

            var complaintOrders = orders
                .Where(o => o.Status == OrderStatusEnum.Returned ||
                           o.Status == OrderStatusEnum.Cancelled)
                .ToList();

            var totalComplaints = complaintOrders.Count;
            var totalOrders = orders.Count;
            var complaintRate = totalOrders > 0
                ? Math.Round(((decimal)totalComplaints / totalOrders) * 100, 2)
                : 0;

            var complaintsBySeller = complaintOrders
                .GroupBy(o => new { o.ShopId, o.Shop.ShopName })
                .Select(g =>
                {
                    var shopOrders = orders.Where(o => o.ShopId == g.Key.ShopId).Count();
                    return new ComplaintBySellerItem
                    {
                        ShopId = g.Key.ShopId,
                        ShopName = g.Key.ShopName,
                        ComplaintCount = g.Count(),
                        TotalOrders = shopOrders,
                        ComplaintRate = shopOrders > 0
                            ? Math.Round(((decimal)g.Count() / shopOrders) * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(c => c.ComplaintRate)
                .ToList();

            var statusBreakdown = complaintOrders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var response = new ComplaintStatisticsResponse
            {
                TotalComplaints = totalComplaints,
                TotalOrders = totalOrders,
                ComplaintRate = complaintRate,
                ComplaintsBySeller = complaintsBySeller,
                ComplaintStatusBreakdown = statusBreakdown
            };

            return result.BuildSuccess(response, "Complaint statistics retrieved successfully");
        }
    }

    public class GetSellerHealthScoreQueryHandler : IRequestHandler<GetSellerHealthScoreQuery, BaseResponse<List<SellerHealthScoreResponse>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;

        public GetSellerHealthScoreQueryHandler(
            IOrderRepository orderRepository,
            IShopRepository shopRepository)
        {
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BaseResponse<List<SellerHealthScoreResponse>>> Handle(
            GetSellerHealthScoreQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<SellerHealthScoreResponse>>();

            var shopsQuery = _shopRepository.GetAllQueryable();

            if (request.ShopId.HasValue)
            {
                shopsQuery = shopsQuery.Where(s => s.Id == request.ShopId.Value);
            }

            var shops = await shopsQuery.Take(request.TopN).ToListAsync(cancellationToken);

            var healthScores = new List<SellerHealthScoreResponse>();

            foreach (var shop in shops)
            {
                var orders = await _orderRepository.GetAllQueryable()
                    .Where(o => o.ShopId == shop.Id)
                    .ToListAsync(cancellationToken);

                if (orders.Count == 0)
                {
                    healthScores.Add(new SellerHealthScoreResponse
                    {
                        ShopId = shop.Id,
                        ShopName = shop.ShopName,
                        HealthScore = 0,
                        HealthStatus = "No Data",
                        Breakdown = new HealthScoreBreakdown()
                    });
                    continue;
                }

                var totalOrders = orders.Count;
                var completedOrders = orders.Count(o => o.Status == OrderStatusEnum.Completed);
                var cancelledOrders = orders.Count(o => o.Status == OrderStatusEnum.Cancelled);
                var returnedOrders = orders.Count(o => o.Status == OrderStatusEnum.Returned);

                var completionRate = totalOrders > 0
                    ? ((decimal)completedOrders / totalOrders) * 100
                    : 0;

                var cancellationRate = totalOrders > 0
                    ? ((decimal)cancelledOrders / totalOrders) * 100
                    : 0;

                var complaintRate = totalOrders > 0
                    ? ((decimal)returnedOrders / totalOrders) * 100
                    : 0;

                var processingTimes = orders
                    .Where(o => o.AcceptedAt.HasValue && o.Status == OrderStatusEnum.Completed)
                    .Select(o => (o.CompletedAt ?? DateTime.UtcNow) - o.AcceptedAt!.Value)
                    .ToList();

                var avgProcessingHours = processingTimes.Any()
                    ? processingTimes.Average(t => t.TotalHours)
                    : 0;

                var completionRateScore = Math.Min(completionRate, 100m) * 0.4m;
                var cancellationRateScore = Math.Max(100 - cancellationRate, 0) * 0.3m;
                var complaintRateScore = Math.Max(100 - complaintRate, 0) * 0.2m;

                var speedScore = avgProcessingHours > 0
                    ? Math.Max(100 - (decimal)(avgProcessingHours / 24 * 10), 0) * 0.1m
                    : 0;

                var totalScore = completionRateScore + cancellationRateScore + complaintRateScore + speedScore;

                var healthStatus = totalScore switch
                {
                    >= 80 => "Excellent",
                    >= 60 => "Good",
                    >= 40 => "Fair",
                    >= 20 => "Poor",
                    _ => "Critical"
                };

                healthScores.Add(new SellerHealthScoreResponse
                {
                    ShopId = shop.Id,
                    ShopName = shop.ShopName,
                    HealthScore = Math.Round(totalScore, 2),
                    HealthStatus = healthStatus,
                    Breakdown = new HealthScoreBreakdown
                    {
                        CompletionRateScore = Math.Round(completionRateScore, 2),
                        CancellationRateScore = Math.Round(cancellationRateScore, 2),
                        ComplaintRateScore = Math.Round(complaintRateScore, 2),
                        ProcessingSpeedScore = Math.Round(speedScore, 2),
                        CompletionRate = Math.Round(completionRate, 2),
                        CancellationRate = Math.Round(cancellationRate, 2),
                        ComplaintRate = Math.Round(complaintRate, 2),
                        AverageProcessingHours = Math.Round(avgProcessingHours, 2)
                    }
                });
            }

            healthScores = healthScores.OrderByDescending(h => h.HealthScore).ToList();

            return result.BuildSuccess(healthScores, "Seller health scores retrieved successfully");
        }
    }

    public class GetBuyerRiskScoreQueryHandler : IRequestHandler<GetBuyerRiskScoreQuery, BaseResponse<List<BuyerRiskScoreResponse>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public GetBuyerRiskScoreQueryHandler(
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<List<BuyerRiskScoreResponse>>> Handle(
            GetBuyerRiskScoreQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<BuyerRiskScoreResponse>>();

            var buyersQuery = _userRepository.GetAllQueryable()
                .Include(u => u.UserRoles)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == RoleEnum.Buyer));

            if (request.BuyerId.HasValue)
            {
                buyersQuery = buyersQuery.Where(u => u.Id == request.BuyerId.Value);
            }

            var buyers = await buyersQuery.ToListAsync(cancellationToken);

            var riskScores = new List<BuyerRiskScoreResponse>();

            foreach (var buyer in buyers)
            {
                var orders = await _orderRepository.GetAllQueryable()
                    .Where(o => o.BuyerId == buyer.Id)
                    .ToListAsync(cancellationToken);

                if (orders.Count == 0) continue;

                var totalOrders = orders.Count;
                var refundCount = orders.Count(o => o.Status == OrderStatusEnum.Returned);
                var complaintCount = orders.Count(o => !string.IsNullOrEmpty(o.CancelReason));
                var cancellationCount = orders.Count(o => o.Status == OrderStatusEnum.Cancelled);

                var refundRate = totalOrders > 0
                    ? ((decimal)refundCount / totalOrders) * 100
                    : 0;

                var complaintRate = totalOrders > 0
                    ? ((decimal)complaintCount / totalOrders) * 100
                    : 0;

                var cancellationRate = totalOrders > 0
                    ? ((decimal)cancellationCount / totalOrders) * 100
                    : 0;

                var riskScore = (refundRate * 0.4m) + (complaintRate * 0.3m) + (cancellationRate * 0.3m);

                var riskLevel = riskScore switch
                {
                    >= 50 => "High Risk",
                    >= 30 => "Medium Risk",
                    >= 10 => "Low Risk",
                    _ => "Minimal Risk"
                };

                riskScores.Add(new BuyerRiskScoreResponse
                {
                    BuyerId = buyer.Id,
                    BuyerName = buyer.FullName,
                    RiskScore = Math.Round(riskScore, 2),
                    RiskLevel = riskLevel,
                    Breakdown = new RiskScoreBreakdown
                    {
                        RefundCount = refundCount,
                        ComplaintCount = complaintCount,
                        CancellationCount = cancellationCount,
                        TotalOrders = totalOrders,
                        RefundRate = Math.Round(refundRate, 2),
                        ComplaintRate = Math.Round(complaintRate, 2),
                        CancellationRate = Math.Round(cancellationRate, 2)
                    }
                });
            }

            riskScores = riskScores
                .OrderByDescending(r => r.RiskScore)
                .Take(request.TopN)
                .ToList();

            return result.BuildSuccess(riskScores, "Buyer risk scores retrieved successfully");
        }
    }
}
