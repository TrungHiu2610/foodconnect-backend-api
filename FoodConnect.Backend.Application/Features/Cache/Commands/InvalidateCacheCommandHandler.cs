using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Helpers;
using FoodConnect.Backend.Application.Commons.Interfaces;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cache.Commands
{
    public class InvalidateCacheCommandHandler : IRequestHandler<InvalidateCacheCommand, BaseResponse<string>>
    {
        private readonly IRedisService _redisService;

        public InvalidateCacheCommandHandler(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<BaseResponse<string>> Handle(InvalidateCacheCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<string>();
            long deletedCount = 0;

            if (request.InvalidateAll)
            {
                // Invalidate all cache patterns
                deletedCount += await _redisService.DeleteByPatternAsync("seller:*");
                deletedCount += await _redisService.DeleteByPatternAsync("admin:*");
                deletedCount += await _redisService.DeleteByPatternAsync("buyer:*");
            }
            else if (!string.IsNullOrWhiteSpace(request.CachePattern))
            {
                // Invalidate specific pattern
                deletedCount = await _redisService.DeleteByPatternAsync(request.CachePattern);
            }
            else
            {
                // Invalidate seller-specific cache
                if (request.ShopId.HasValue)
                {
                    var sellerPattern = $"seller:{request.ShopId}:*";
                    deletedCount += await _redisService.DeleteByPatternAsync(sellerPattern);
                }

                // Invalidate buyer-specific cache
                if (request.BuyerId.HasValue)
                {
                    var buyerPattern = $"buyer:{request.BuyerId}:*";
                    deletedCount += await _redisService.DeleteByPatternAsync(buyerPattern);
                }

                // If shopId or buyerId affected, also invalidate admin statistics
                if (request.ShopId.HasValue || request.BuyerId.HasValue)
                {
                    deletedCount += await _redisService.DeleteByPatternAsync("admin:statistics:*");
                }
            }

            var message = $"Successfully invalidated {deletedCount} cache entries";
            return result.BuildSuccess(message, message);
        }
    }
}
