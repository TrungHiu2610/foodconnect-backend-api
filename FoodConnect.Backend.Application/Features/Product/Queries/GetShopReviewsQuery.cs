using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetShopReviewsQuery : IRequest<BaseResponse<PagedResponse<ProductReviewResponse>>>
    {
        public Guid ShopId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? MinRating { get; set; } // Filter by minimum rating (1-5)
        public bool? HasSellerResponse { get; set; } // Filter by seller response status
    }
}
