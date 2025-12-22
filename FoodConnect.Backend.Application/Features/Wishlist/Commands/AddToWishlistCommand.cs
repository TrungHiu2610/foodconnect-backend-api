using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class AddToWishlistCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid? ProductId { get; set; }
        public Guid? ShopId { get; set; }
        public bool NotificationEnabled { get; set; } = true;
    }
}
