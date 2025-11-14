using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wishlist;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Queries
{
    public class GetMyWishlistQuery : IRequest<BaseResponse<List<WishlistResponse>>>
    {
        public int? Type { get; set; }
    }
}
