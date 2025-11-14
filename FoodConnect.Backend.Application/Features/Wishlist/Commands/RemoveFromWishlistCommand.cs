using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class RemoveFromWishlistCommand : IRequest<BaseResponse<DeleteResponse>>
    {
        public Guid WishlistId { get; set; }
    }
}
