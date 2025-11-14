using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class UpdateWishlistNotificationCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
    {
        public Guid WishlistId { get; set; }
        public bool NotificationEnabled { get; set; }
    }
}
