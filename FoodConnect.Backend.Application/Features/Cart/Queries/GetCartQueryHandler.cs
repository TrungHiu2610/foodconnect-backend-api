using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    /// <summary>
    /// Get cart for Cart Page - simple view with items grouped by shop only
    /// </summary>
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
                    Summary = new CartSummary()
                }, "Empty cart");
            }

            if (cart == null)
            {
                return result.BuildSuccess(new CartResponse
                {
                    ShopGroups = new List<ShopCartGroup>(),
                    Summary = new CartSummary()
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
                // Sort cart items by CreatedAtUtc to maintain order (like Shopee)
                var sortedCartItems = cart.CartItems
                    .Where(item => item.Product != null && item.Product.Shop != null)
                    .OrderBy(item => item.CreatedAtUtc)
                    .ToList();

                // Group by Shop only (Cart Page doesn't show delivery type grouping)
                var groupedByShop = sortedCartItems
                    .GroupBy(item => new
                    {
                        ShopId = item.Product!.ShopId,
                        ShopName = item.Product.Shop!.ShopName,
                        ShopStatus = item.Product.Shop.Status,
                        FirstItemCreatedAt = sortedCartItems
                            .Where(ci => ci.Product!.ShopId == item.Product!.ShopId)
                            .Min(ci => ci.CreatedAtUtc)
                    })
                    .OrderBy(g => g.Key.FirstItemCreatedAt);

                foreach (var shopGroup in groupedByShop)
                {
                    var shopCartGroup = new ShopCartGroup
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        ShopStatus = shopGroup.Key.ShopStatus.ToString(),
                        Items = new List<CartItemResponse>()
                    };

                    // Add all items from this shop (maintain creation order)
                    var sortedShopItems = shopGroup.OrderBy(item => item.CreatedAtUtc);

                    foreach (var item in sortedShopItems)
                    {
                        var product = item.Product;

                        var cartItem = new CartItemResponse
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = product?.Name ?? string.Empty,
                            ProductThumbnail = product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                            ProductPrice = product?.Price ?? 0,
                            Quantity = item.Quantity,
                            Subtotal = (product?.Price ?? 0) * item.Quantity,
                            AvailableStock = product?.StockQuantity ?? 0,
                            IsAvailable = product?.IsAvailable ?? false && product?.Status == Domain.Enums.ProductStatusEnum.Active
                        };

                        shopCartGroup.Items.Add(cartItem);
                    }

                    shopCartGroup.ShopSubtotal = shopCartGroup.Items.Sum(i => i.Subtotal);

                    response.ShopGroups.Add(shopCartGroup);
                }
            }

            // Calculate simple summary (no shipping fees for Cart Page)
            var allItems = response.ShopGroups.SelectMany(s => s.Items).ToList();

            response.Summary = new CartSummary
            {
                TotalItems = allItems.Count,
                TotalAmount = allItems.Sum(i => i.Subtotal)
            };

            return response;
        }
    }
}
