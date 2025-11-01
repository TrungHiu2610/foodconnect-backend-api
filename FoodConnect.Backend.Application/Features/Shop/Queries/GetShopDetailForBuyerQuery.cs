using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    /// <summary>
    /// Query to get detailed shop information for buyer view
    /// Returns shop details, operating hours, and available (Active) products
    /// </summary>
    public class GetShopDetailForBuyerQuery : IRequest<BaseResponse<ShopDetailForBuyerResponse>>
    {
        /// <summary>
        /// Shop ID to retrieve details for
        /// </summary>
        public Guid ShopId { get; set; }

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
