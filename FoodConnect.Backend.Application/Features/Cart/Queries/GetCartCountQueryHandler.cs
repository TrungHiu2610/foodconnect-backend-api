using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCartCountQueryHandler : IRequestHandler<GetCartCountQuery, BaseResponse<CartCountResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCartCountQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CartCountResponse>> Handle(GetCartCountQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartCountResponse>();

            var userId = _currentUserService.UserId;

            Domain.Entities.Cart? cart;
            if (userId.HasValue)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(userId, null);
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                cart = await _cartRepository.GetCartWithItemsAsync(null, request.SessionId);
            }
            else
            {
                return result.BuildSuccess(new CartCountResponse
                {
                    ItemCount = 0,
                    TotalQuantity = 0
                }, "Empty cart");
            }

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return result.BuildSuccess(new CartCountResponse
                {
                    ItemCount = 0,
                    TotalQuantity = 0
                }, "Empty cart");
            }

            var response = new CartCountResponse
            {
                ItemCount = cart.CartItems.Count,
                TotalQuantity = cart.CartItems.Sum(i => i.Quantity)
            };

            return result.BuildSuccess(response, "Get cart count successfully");
        }
    }
}
