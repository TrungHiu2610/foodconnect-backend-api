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
                deletedCount += await _redisService.DeleteByPatternAsync("seller:*");
                deletedCount += await _redisService.DeleteByPatternAsync("admin:*");
                deletedCount += await _redisService.DeleteByPatternAsync("buyer:*");
            }
            else if (!string.IsNullOrWhiteSpace(request.CachePattern))
            {
                deletedCount = await _redisService.DeleteByPatternAsync(request.CachePattern);
            }
            else
            {
                if (request.ShopId.HasValue)
                {
                    var sellerPattern = $"seller:{request.ShopId}:*";
                    deletedCount += await _redisService.DeleteByPatternAsync(sellerPattern);
                }

                if (request.BuyerId.HasValue)
                {
                    var buyerPattern = $"buyer:{request.BuyerId}:*";
                    deletedCount += await _redisService.DeleteByPatternAsync(buyerPattern);
                }

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
