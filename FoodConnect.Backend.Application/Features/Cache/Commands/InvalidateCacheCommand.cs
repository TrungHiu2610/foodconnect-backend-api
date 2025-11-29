using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cache.Commands
{
    public class InvalidateCacheCommand : IRequest<BaseResponse<string>>
    {
        public string CachePattern { get; set; } = string.Empty;
        public Guid? ShopId { get; set; }
        public Guid? BuyerId { get; set; }
        public bool InvalidateAll { get; set; } = false;
    }
}
