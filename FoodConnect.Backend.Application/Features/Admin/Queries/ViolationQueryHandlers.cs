using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetAccountViolationListQueryHandler : IRequestHandler<GetAccountViolationListQuery, BaseResponse<List<AccountViolationListResponse>>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisService _redisService;

        // Thresholds for warning badges
        private const decimal SELLER_WARNING_THRESHOLD = 40; // Health score < 40
        private const decimal BUYER_WARNING_THRESHOLD = 30; // Risk score >= 30

        public GetAccountViolationListQueryHandler(
            IUserRepository userRepository,
            IShopRepository shopRepository,
            IOrderRepository orderRepository,
            IRedisService redisService)
        {
            _userRepository = userRepository;
            _shopRepository = shopRepository;
            _orderRepository = orderRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<List<AccountViolationListResponse>>> Handle(
            GetAccountViolationListQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<AccountViolationListResponse>>();

            // Check cache
            var cacheKey = CacheKeys.AccountViolationList(request.Role, request.MinScore, request.MaxScore, 
                request.HasWarningBadge, request.PageNumber, request.PageSize, request.SortBy, request.SortOrder);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<List<AccountViolationListResponse>>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var usersQuery = _userRepository.GetAllQueryable()
                .Include(u => u.UserRoles)
                .AsQueryable();

            // Filter by role if specified
            if (!string.IsNullOrEmpty(request.Role))
            {
                var roleEnum = request.Role.ToLower() == "seller" ? RoleEnum.Seller : RoleEnum.Buyer;
                usersQuery = usersQuery.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleEnum));
            }

            var users = await usersQuery.ToListAsync(cancellationToken);
            var responses = new List<AccountViolationListResponse>();

            foreach (var user in users)
            {
                var userRoles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();
                var isSeller = user.UserRoles.Any(ur => ur.RoleId == RoleEnum.Seller);
                var isBuyer = user.UserRoles.Any(ur => ur.RoleId == RoleEnum.Buyer);

                var orders = await _orderRepository.GetAllQueryable()
                    .Where(o => (isSeller && o.Shop.UserId == user.Id) || (isBuyer && o.BuyerId == user.Id))
                    .ToListAsync(cancellationToken);

                var lastOrderDate = orders.Any() ? orders.Max(o => o.CreatedAtUtc) : (DateTime?)null;

                var response = new AccountViolationListResponse
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    UserStatus = (int)user.Status,
                    UserStatusName = user.Status.ToString(),
                    Roles = userRoles,
                    CreatedAt = user.CreatedAtUtc,
                    LastOrderDate = lastOrderDate
                };

                // Calculate Seller metrics if user is seller
                if (isSeller)
                {
                    var shop = await _shopRepository.GetAllQueryable()
                        .FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);

                    if (shop != null)
                    {
                        var shopOrders = orders.Where(o => o.ShopId == shop.Id).ToList();
                        
                        if (shopOrders.Any())
                        {
                            var totalOrders = shopOrders.Count;
                            var completedOrders = shopOrders.Count(o => o.Status == OrderStatusEnum.Completed);
                            var cancelledOrders = shopOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);
                            var returnedOrders = shopOrders.Count(o => o.Status == OrderStatusEnum.Returned);

                            var completionRate = totalOrders > 0
                                ? ((decimal)completedOrders / totalOrders) * 100
                                : 0;

                            var cancellationRate = totalOrders > 0
                                ? ((decimal)cancelledOrders / totalOrders) * 100
                                : 0;

                            var complaintRate = totalOrders > 0
                                ? ((decimal)returnedOrders / totalOrders) * 100
                                : 0;

                            // Calculate health score using same logic as GetSellerHealthScoreQuery
                            var processingTimes = shopOrders
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

                            var healthScore = completionRateScore + cancellationRateScore + complaintRateScore + speedScore;

                            var healthStatus = healthScore switch
                            {
                                >= 80 => "Excellent",
                                >= 60 => "Good",
                                >= 40 => "Fair",
                                >= 20 => "Poor",
                                _ => "Critical"
                            };

                            response.SellerMetrics = new SellerViolationMetrics
                            {
                                ShopId = shop.Id,
                                ShopName = shop.ShopName,
                                HealthScore = Math.Round(healthScore, 2),
                                HealthStatus = healthStatus,
                                CancellationRate = Math.Round(cancellationRate, 2),
                                ComplaintRate = Math.Round(complaintRate, 2),
                                CompletionRate = Math.Round(completionRate, 2),
                                TotalOrders = totalOrders,
                                CancelledOrders = cancelledOrders,
                                ComplaintOrders = returnedOrders
                            };

                            // Check warning threshold for seller
                            if (healthScore < SELLER_WARNING_THRESHOLD)
                            {
                                response.HasWarningBadge = true;
                                response.WarningReason = $"Low health score ({Math.Round(healthScore, 2)}). High cancellation/complaint rate.";
                            }
                        }
                    }
                }

                // Calculate Buyer metrics if user is buyer
                if (isBuyer)
                {
                    var buyerOrders = orders.Where(o => o.BuyerId == user.Id).ToList();
                    
                    if (buyerOrders.Any())
                    {
                        var totalOrders = buyerOrders.Count;
                        var refundCount = buyerOrders.Count(o => o.Status == OrderStatusEnum.Returned);
                        var complaintCount = buyerOrders.Count(o => !string.IsNullOrEmpty(o.CancelReason));
                        var cancellationCount = buyerOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);

                        var refundRate = totalOrders > 0
                            ? ((decimal)refundCount / totalOrders) * 100
                            : 0;

                        var complaintRate = totalOrders > 0
                            ? ((decimal)complaintCount / totalOrders) * 100
                            : 0;

                        var cancellationRate = totalOrders > 0
                            ? ((decimal)cancellationCount / totalOrders) * 100
                            : 0;

                        // Calculate risk score using same logic as GetBuyerRiskScoreQuery
                        var riskScore = (refundRate * 0.4m) + (complaintRate * 0.3m) + (cancellationRate * 0.3m);

                        var riskLevel = riskScore switch
                        {
                            >= 50 => "High Risk",
                            >= 30 => "Medium Risk",
                            >= 10 => "Low Risk",
                            _ => "Minimal Risk"
                        };

                        response.BuyerMetrics = new BuyerViolationMetrics
                        {
                            RiskScore = Math.Round(riskScore, 2),
                            RiskLevel = riskLevel,
                            CancellationRate = Math.Round(cancellationRate, 2),
                            RefundRate = Math.Round(refundRate, 2),
                            ComplaintRate = Math.Round(complaintRate, 2),
                            TotalOrders = totalOrders,
                            CancelledOrders = cancellationCount,
                            RefundOrders = refundCount,
                            ComplaintOrders = complaintCount
                        };

                        // Check warning threshold for buyer
                        if (riskScore >= BUYER_WARNING_THRESHOLD)
                        {
                            response.HasWarningBadge = true;
                            response.WarningReason = $"High risk score ({Math.Round(riskScore, 2)}). Frequent order cancellations/returns.";
                        }
                    }
                }

                responses.Add(response);
            }

            // Apply filters
            if (request.HasWarningBadge.HasValue)
            {
                responses = responses.Where(r => r.HasWarningBadge == request.HasWarningBadge.Value).ToList();
            }

            if (request.MinScore.HasValue || request.MaxScore.HasValue)
            {
                responses = responses.Where(r =>
                {
                    decimal? score = null;
                    if (r.SellerMetrics != null) score = r.SellerMetrics.HealthScore;
                    else if (r.BuyerMetrics != null) score = r.BuyerMetrics.RiskScore;

                    if (!score.HasValue) return false;

                    if (request.MinScore.HasValue && score < request.MinScore) return false;
                    if (request.MaxScore.HasValue && score > request.MaxScore) return false;
                    return true;
                }).ToList();
            }

            // Apply sorting
            responses = request.SortBy?.ToLower() switch
            {
                "name" => request.SortOrder?.ToLower() == "asc"
                    ? responses.OrderBy(r => r.FullName).ToList()
                    : responses.OrderByDescending(r => r.FullName).ToList(),
                "createdat" => request.SortOrder?.ToLower() == "asc"
                    ? responses.OrderBy(r => r.CreatedAt).ToList()
                    : responses.OrderByDescending(r => r.CreatedAt).ToList(),
                _ => request.SortOrder?.ToLower() == "asc"
                    ? responses.OrderBy(r => r.SellerMetrics?.HealthScore ?? r.BuyerMetrics?.RiskScore ?? 0).ToList()
                    : responses.OrderByDescending(r => r.SellerMetrics?.HealthScore ?? r.BuyerMetrics?.RiskScore ?? 0).ToList()
            };

            // Apply pagination
            var paginatedResponses = responses
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var successResult = result.BuildSuccess(paginatedResponses, "Account violation list retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminQuality);
            return successResult;
        }
    }

    public class GetAccountActivityDetailQueryHandler : IRequestHandler<GetAccountActivityDetailQuery, BaseResponse<AccountActivityDetailResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRedisService _redisService;

        public GetAccountActivityDetailQueryHandler(
            IUserRepository userRepository,
            IShopRepository shopRepository,
            IOrderRepository orderRepository,
            IRedisService redisService)
        {
            _userRepository = userRepository;
            _shopRepository = shopRepository;
            _orderRepository = orderRepository;
            _redisService = redisService;
        }

        public async Task<BaseResponse<AccountActivityDetailResponse>> Handle(
            GetAccountActivityDetailQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AccountActivityDetailResponse>();

            // Check cache
            var cacheKey = CacheKeys.AccountActivityDetail(request.UserId, request.RecentOrdersLimit);
            var cachedResponse = await _redisService.GetAsync<BaseResponse<AccountActivityDetailResponse>>(cacheKey);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }

            var user = await _userRepository.GetAllQueryable()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return result.BuildNotFound();
            }

            var isSeller = user.UserRoles.Any(ur => ur.RoleId == RoleEnum.Seller);
            var isBuyer = user.UserRoles.Any(ur => ur.RoleId == RoleEnum.Buyer);

            var response = new AccountActivityDetailResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserStatus = (int)user.Status,
                UserStatusName = user.Status.ToString(),
                Roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList()
            };

            // Get all orders related to this user
            var ordersQuery = _orderRepository.GetAllQueryable()
                .Include(o => o.Shop)
                .Include(o => o.Buyer)
                .AsQueryable();

            if (isSeller)
            {
                var shop = await _shopRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(s => s.UserId == user.Id, cancellationToken);

                if (shop != null)
                {
                    ordersQuery = ordersQuery.Where(o => o.ShopId == shop.Id);
                }
            }
            else if (isBuyer)
            {
                ordersQuery = ordersQuery.Where(o => o.BuyerId == user.Id);
            }

            var allOrders = await ordersQuery.ToListAsync(cancellationToken);

            // Recent orders
            var recentOrders = allOrders
                .OrderByDescending(o => o.CreatedAtUtc)
                .Take(request.RecentOrdersLimit)
                .Select(o =>
                {
                    var isViolation = o.Status == OrderStatusEnum.Cancelled ||
                                    o.Status == OrderStatusEnum.Returned ||
                                    !string.IsNullOrEmpty(o.CancelReason);

                    string? violationType = null;
                    if (o.Status == OrderStatusEnum.Cancelled) violationType = "Cancelled";
                    else if (o.Status == OrderStatusEnum.Returned) violationType = "Returned";
                    else if (!string.IsNullOrEmpty(o.CancelReason)) violationType = "Complaint";

                    return new RecentOrderActivity
                    {
                        OrderId = o.Id,
                        OrderCode = o.OrderCode,
                        TotalAmount = (decimal)o.Total,
                        OrderStatus = (int)o.Status,
                        OrderStatusName = o.Status.ToString(),
                        CreatedAt = o.CreatedAtUtc,
                        CompletedAt = o.CompletedAt,
                        CancelledAt = o.CancelledAt,
                        CancelReason = o.CancelReason,
                        BuyerName = isSeller ? o.Buyer?.FullName : null,
                        ShopName = isBuyer ? o.Shop?.ShopName : null,
                        ShopId = isBuyer ? o.ShopId : null,
                        IsViolation = isViolation,
                        ViolationType = violationType
                    };
                })
                .ToList();

            response.RecentOrders = recentOrders;

            // Calculate statistics
            var totalOrders = allOrders.Count;
            var completedOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Completed);
            var cancelledOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Cancelled);
            var refundedOrders = allOrders.Count(o => o.Status == OrderStatusEnum.Returned);

            var totalAmount = allOrders.Where(o => o.Status == OrderStatusEnum.Completed).Sum(o => (decimal)o.Total);
            var avgOrderValue = totalOrders > 0 ? totalAmount / totalOrders : 0;

            var firstOrderDate = allOrders.Any() ? allOrders.Min(o => o.CreatedAtUtc) : (DateTime?)null;
            var lastOrderDate = allOrders.Any() ? allOrders.Max(o => o.CreatedAtUtc) : (DateTime?)null;
            var daysSinceFirstOrder = firstOrderDate.HasValue
                ? (DateTime.UtcNow - firstOrderDate.Value).Days
                : 0;

            response.Statistics = new AccountStatisticsSummary
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                RefundedOrders = refundedOrders,
                TotalSpent = isBuyer ? totalAmount : 0,
                TotalRevenue = isSeller ? totalAmount : 0,
                AverageOrderValue = Math.Round(avgOrderValue, 2),
                FirstOrderDate = firstOrderDate,
                LastOrderDate = lastOrderDate,
                DaysSinceFirstOrder = daysSinceFirstOrder
            };

            // Violation history
            var violationHistory = allOrders
                .Where(o => o.Status == OrderStatusEnum.Cancelled ||
                           o.Status == OrderStatusEnum.Returned ||
                           !string.IsNullOrEmpty(o.CancelReason))
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new ViolationHistoryItem
                {
                    OrderId = o.Id,
                    OrderCode = o.OrderCode,
                    ViolationType = o.Status == OrderStatusEnum.Cancelled ? "Cancelled" :
                                  o.Status == OrderStatusEnum.Returned ? "Returned" : "Complaint",
                    Reason = o.CancelReason,
                    ViolationDate = o.Status == OrderStatusEnum.Cancelled 
                        ? (o.CancelledAt ?? o.UpdatedAtUtc ?? o.CreatedAtUtc)
                        : (o.UpdatedAtUtc ?? o.CreatedAtUtc),
                    OrderAmount = (decimal)o.Total
                })
                .ToList();

            response.ViolationHistory = violationHistory;

            var successResult = result.BuildSuccess(response, "Account activity details retrieved successfully");
            await _redisService.SetAsync(cacheKey, successResult, CacheKeys.Expiration.AdminQuality);
            return successResult;
        }
    }
}
