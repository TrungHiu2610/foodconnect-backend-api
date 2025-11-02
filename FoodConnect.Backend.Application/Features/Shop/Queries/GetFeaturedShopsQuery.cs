using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    /// <summary>
    /// Query to get featured shops for buyer view
    /// Returns shops with IsFeatured=true AND Status=Active
    /// Sorted by Rating DESC then TotalOrders DESC
    /// </summary>
    public class GetFeaturedShopsQuery : IRequest<BaseResponse<List<ShopListForBuyerResponse>>>
    {
        /// <summary>
        /// Maximum number of shops to return (default: 10)
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Latitude of buyer's location (optional for distance calculation)
        /// </summary>
        public double? UserLatitude { get; set; }

        /// <summary>
        /// Longitude of buyer's location (optional for distance calculation)
        /// </summary>
        public double? UserLongitude { get; set; }
    }
}
