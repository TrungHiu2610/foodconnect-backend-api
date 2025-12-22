using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class ReorderCommandHandler : IRequestHandler<ReorderCommand, BaseResponse<CartResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public ReorderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<CartResponse>> Handle(ReorderCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

            // Validate user is logged in
            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in to reorder");
            }

            var buyerId = _currentUserService.UserId.Value;

            // Get the original order with items
            var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
            if (order == null)
            {
                return result.BuildNotFound("Order not found");
            }

            // Verify the order belongs to the current user
            if (order.BuyerId != buyerId)
            {
                return result.BuildUnauthorized("You can only reorder your own orders");
            }

            // Check if order has items
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                return result.BuildFail("Order has no items to reorder");
            }

            try
            {
                // Get or create cart for the user
                var cart = await _cartRepository.GetCartWithItemsAsync(buyerId, null);
                if (cart == null)
                {
                    cart = new Domain.Entities.Cart
                    {
                        UserId = buyerId,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    await _cartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                }

                var unavailableProducts = new List<string>();
                var outOfStockProducts = new List<string>();
                var addedProducts = new List<string>();

                // Process each order item
                foreach (var orderItem in order.OrderItems)
                {
                    // Get product
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                    if (product == null)
                    {
                        unavailableProducts.Add($"Product ID {orderItem.ProductId}");
                        continue;
                    }

                    // Check availability
                    if (!product.IsAvailable)
                    {
                        unavailableProducts.Add(product.Name);
                        continue;
                    }

                    // Check stock availability
                    var requestedQuantity = orderItem.Quantity;
                    if (product.StockQuantity.HasValue && product.StockQuantity.Value < requestedQuantity)
                    {
                        // Add what's available if stock is low
                        if (product.StockQuantity.Value > 0)
                        {
                            requestedQuantity = product.StockQuantity.Value;
                            outOfStockProducts.Add($"{product.Name} (chỉ còn {product.StockQuantity.Value})");
                        }
                        else
                        {
                            outOfStockProducts.Add($"{product.Name} (hết hàng)");
                            continue;
                        }
                    }

                    // Check if product already in cart
                    var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == orderItem.ProductId);
                    if (existingCartItem != null)
                    {
                        // Validate total quantity doesn't exceed stock
                        var totalQuantity = existingCartItem.Quantity + requestedQuantity;
                        if (product.StockQuantity.HasValue && totalQuantity > product.StockQuantity.Value)
                        {
                            // Only add up to available stock
                            var canAdd = product.StockQuantity.Value - existingCartItem.Quantity;
                            if (canAdd > 0)
                            {
                                existingCartItem.Quantity += canAdd;
                                existingCartItem.UpdatedAtUtc = DateTime.UtcNow;
                                outOfStockProducts.Add($"{product.Name} (thêm {canAdd}, tối đa {product.StockQuantity.Value})");
                            }
                            else
                            {
                                outOfStockProducts.Add($"{product.Name} (đã có {existingCartItem.Quantity} trong giỏ, không thể thêm)");
                                continue;
                            }
                        }
                        else
                        {
                            // Update quantity (add to existing)
                            existingCartItem.Quantity += requestedQuantity;
                            existingCartItem.UpdatedAtUtc = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Add new cart item
                        var newCartItem = new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = orderItem.ProductId,
                            Quantity = requestedQuantity,
                            CreatedAtUtc = DateTime.UtcNow,
                            UpdatedAtUtc = DateTime.UtcNow
                        };
                        await _cartItemRepository.AddAsync(newCartItem);
                    }

                    addedProducts.Add(product.Name);
                }

                await _unitOfWork.SaveChangesAsync();

                // Reload cart with full details
                cart = await _cartRepository.GetCartWithItemsAsync(buyerId, null);
                var cartResponse = MapCartToResponse(cart!);

                // Build response message with warnings if any
                var messages = new List<string>();
                if (addedProducts.Any())
                {
                    messages.Add($"✅ Đã thêm {addedProducts.Count} sản phẩm vào giỏ hàng");
                }
                if (unavailableProducts.Any())
                {
                    messages.Add($"⚠️ {unavailableProducts.Count} sản phẩm không còn bán: {string.Join(", ", unavailableProducts.Take(3))}{(unavailableProducts.Count > 3 ? "..." : "")}");
                }
                if (outOfStockProducts.Any())
                {
                    messages.Add($"⚠️ {outOfStockProducts.Count} sản phẩm hết hàng hoặc không đủ số lượng: {string.Join(", ", outOfStockProducts.Take(3))}{(outOfStockProducts.Count > 3 ? "..." : "")}");
                }

                var message = string.Join("\n", messages);

                return result.BuildSuccess(cartResponse, message);
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Reorder failed: {ex.Message}");
            }
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
                var sortedCartItems = cart.CartItems
                    .Where(item => item.Product != null && item.Product.Shop != null)
                    .OrderBy(item => item.CreatedAtUtc)
                    .ToList();

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

                    decimal shopSubtotal = 0;

                    foreach (var cartItem in shopGroup.OrderBy(ci => ci.CreatedAtUtc))
                    {
                        var itemResponse = new CartItemResponse
                        {
                            Id = cartItem.Id,
                            ProductId = cartItem.ProductId,
                            ProductName = cartItem.Product!.Name,
                            ProductThumbnail = cartItem.Product.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                            ProductPrice = cartItem.Product.Price,
                            Quantity = cartItem.Quantity,
                            Subtotal = cartItem.Product.Price * cartItem.Quantity,
                            AvailableStock = cartItem.Product.StockQuantity ?? 0,
                            IsAvailable = cartItem.Product.IsAvailable
                        };

                        shopSubtotal += itemResponse.Subtotal;
                        shopCartGroup.Items.Add(itemResponse);
                    }

                    shopCartGroup.ShopSubtotal = shopSubtotal;
                    response.ShopGroups.Add(shopCartGroup);
                }
            }

            response.Summary = new CartSummary
            {
                TotalItems = response.ShopGroups.Sum(sg => sg.Items.Sum(i => i.Quantity)),
                TotalAmount = response.ShopGroups.Sum(sg => sg.ShopSubtotal)
            };

            return response;
        }
    }
}
