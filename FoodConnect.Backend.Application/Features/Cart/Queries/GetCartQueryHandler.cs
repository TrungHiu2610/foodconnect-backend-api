using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, BaseResponse<CartResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCartQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CartResponse>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

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
                // Return empty cart for guest without session
                return result.BuildSuccess(new CartResponse
                {
                    Items = new List<CartItemResponse>(),
                    TotalItems = 0,
                    TotalAmount = 0
                }, "Empty cart");
            }

            if (cart == null)
            {
                // Return empty cart
                return result.BuildSuccess(new CartResponse
                {
                    Items = new List<CartItemResponse>(),
                    TotalItems = 0,
                    TotalAmount = 0
                }, "Empty cart");
            }

            var response = MapCartToResponse(cart);

            return result.BuildSuccess(response, "Get cart successfully");
        }

        private CartResponse MapCartToResponse(Domain.Entities.Cart cart)
        {
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId != Guid.Empty ? cart.UserId : null,
                SessionId = cart.SessionId,
                CreatedAtUtc = cart.CreatedAtUtc,
                UpdatedAtUtc = cart.UpdatedAtUtc,
                Items = new List<CartItemResponse>()
            };

            if (cart.CartItems != null)
            {
                foreach (var item in cart.CartItems)
                {
                    var product = item.Product;
                    var shop = product?.Shop;
                    
                    // Get today's availability
                    var todayAvailability = product?.ProductDailyAvailabilities?
                        .FirstOrDefault(a => a.Date.Date == DateTime.UtcNow.Date);
                    
                    response.Items.Add(new CartItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = product?.Name ?? string.Empty,
                        ProductThumbnail = product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                        ProductPrice = product?.Price ?? 0,
                        ProductUnit = product?.Unit ?? string.Empty,
                        Quantity = item.Quantity,
                        Subtotal = (product?.Price ?? 0) * item.Quantity,
                        
                        // Shop info
                        ShopId = product?.ShopId ?? Guid.Empty,
                        ShopName = shop?.ShopName ?? string.Empty,
                        ShopStatus = shop?.Status.ToString() ?? string.Empty,
                        ShopAddress = null,  // Address removed from Shop entity
                        
                        // Product availability
                        AvailableStock = todayAvailability?.Quantity ?? 0,
                        IsAvailable = product?.Status == Domain.Enums.ProductStatusEnum.Active
                    });
                }
            }

            response.TotalItems = response.Items.Sum(i => i.Quantity);
            response.TotalAmount = response.Items.Sum(i => i.Subtotal);

            return response;
        }
    }
}
