using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Models;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetOvernightShopsQuery : IRequest<BaseResponse<PaginatedList<ShopListForBuyerResponse>>>
    {
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
        public double? MaxDistanceKm { get; set; }
        
        public string? TextSearch { get; set; }
        public List<Guid>? CategoryIds { get; set; }
        public decimal? MinRating { get; set; }
        
        public string? SortBy { get; set; } = "distance"; // Default sort by distance
        public bool IsDescending { get; set; } = false;
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
