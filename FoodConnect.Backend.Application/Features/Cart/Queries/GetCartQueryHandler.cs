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
                return result.BuildSuccess(new CartResponse
                {
                    ShopGroups = new List<ShopCartGroup>(),
                    TotalItems = 0,
                    TotalAmount = 0
                }, "Empty cart");
            }

            if (cart == null)
            {
                return result.BuildSuccess(new CartResponse
                {
                    ShopGroups = new List<ShopCartGroup>(),
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
                ShopGroups = new List<ShopCartGroup>()
            };

            if (cart.CartItems != null && cart.CartItems.Any())
            {
                var groupedByShop = cart.CartItems
                    .Where(item => item.Product != null && item.Product.Shop != null)
                    .GroupBy(item => new 
                    { 
                        ShopId = item.Product!.ShopId, 
                        ShopName = item.Product.Shop!.ShopName,
                        ShopStatus = item.Product.Shop.Status
                    });

                foreach (var shopGroup in groupedByShop)
                {
                    var shopCartGroup = new ShopCartGroup
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        ShopStatus = shopGroup.Key.ShopStatus.ToString(),
                        Items = new List<CartItemResponse>()
                    };

                    foreach (var item in shopGroup)
                    {
                        var product = item.Product;
                        var todayAvailability = product?.ProductDailyAvailabilities?
                            .FirstOrDefault(a => a.Date.Date == DateTime.UtcNow.Date);

                        var cartItem = new CartItemResponse
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = product?.Name ?? string.Empty,
                            ProductThumbnail = product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                            ProductPrice = product?.Price ?? 0,
                            ProductUnit = product?.Unit ?? string.Empty,
                            Quantity = item.Quantity,
                            Subtotal = (product?.Price ?? 0) * item.Quantity,
                            AvailableStock = todayAvailability?.Quantity ?? 0,
                            IsAvailable = product?.Status == Domain.Enums.ProductStatusEnum.Active
                        };

                        shopCartGroup.Items.Add(cartItem);
                    }

                    shopCartGroup.ShopSubtotal = shopCartGroup.Items.Sum(i => i.Subtotal);
                    response.ShopGroups.Add(shopCartGroup);
                }
            }

            response.TotalItems = response.ShopGroups.SelectMany(g => g.Items).Sum(i => i.Quantity);
            response.TotalAmount = response.ShopGroups.Sum(g => g.ShopSubtotal);

            return response;
        }
    }
}
