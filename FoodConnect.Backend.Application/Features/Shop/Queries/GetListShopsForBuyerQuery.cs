using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Models;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetListShopsForBuyerQuery : IRequest<BaseResponse<PaginatedList<ShopListForBuyerResponse>>>
    {
        // Filtering
        public string? TextSearch { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public decimal? MinRating { get; set; }
        public double? MaxDistance { get; set; }
        public bool? IsOpen { get; set; }
        public bool? HasPromotion { get; set; }

        // User location for distance calculation
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }

        // Sorting
        public string? SortBy { get; set; } // "rating", "distance", "orders", "newest", "featured"
        public bool IsDescending { get; set; } = true;

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
