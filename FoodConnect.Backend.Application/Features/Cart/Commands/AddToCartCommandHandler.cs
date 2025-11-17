using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Application.Commons.Constants;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Commands
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, BaseResponse<CartResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AddToCartCommandHandler(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CartResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartResponse>();

            // Check product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return result.BuildFail("Product not found");
            }

            var userId = _currentUserService.UserId;

            // Validate SessionId
            if (string.IsNullOrWhiteSpace(request.SessionId) && !userId.HasValue)
            {
                return result.BuildFail("Either user must be logged in or a valid session ID must be provided");
            }

            // Get or create cart
            Domain.Entities.Cart? cart;
            if (userId.HasValue)
            {
                cart = await _cartRepository.GetCartWithItemsAsync(userId, null);
            }
            else
            {
                cart = await _cartRepository.GetCartWithItemsAsync(null, request.SessionId);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Create new cart if not exists
                if (cart == null)
                {
                    cart = new Domain.Entities.Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId, // Set to null for guest users
                        SessionId = request.SessionId!,
                        CartItems = new List<CartItem>()
                    };
                    await _cartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Check if product already in cart
                var existingItem = await _cartItemRepository.GetCartItemAsync(cart.Id, request.ProductId);

                if (existingItem != null)
                {
                    // Check if total quantity exceeds maximum
                    var newQuantity = existingItem.Quantity + request.Quantity;
                    if (newQuantity > CartConstants.MaxQuantityPerProduct)
                    {
                        await transaction.RollbackAsync();
                        return result.BuildFail($"Total quantity cannot exceed {CartConstants.MaxQuantityPerProduct}. Current: {existingItem.Quantity}, Adding: {request.Quantity}");
                    }
                    
                    // Update quantity
                    existingItem.Quantity = newQuantity;
                    _cartItemRepository.Update(existingItem);
                }
                else
                {
                    // Check cart items limit (max products in cart)
                    var currentItemCount = cart.CartItems?.Count ?? 0;
                    if (currentItemCount >= CartConstants.MaxProductsInCart)
                    {
                        await transaction.RollbackAsync();
                        return result.BuildFail($"Cart cannot contain more than {CartConstants.MaxProductsInCart} different products");
                    }
                    
                    // Add new item
                    var cartItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    };
                    await _cartItemRepository.AddAsync(cartItem);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                // Reload cart with full data
                cart = await _cartRepository.GetCartWithItemsAsync(userId, request.SessionId);
                var response = MapCartToResponse(cart!);

                return result.BuildSuccess(response, "Added to cart successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Add to cart failed: {ex.Message}");
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
                // Sort cart items by CreatedAtUtc to maintain order (consistent with GetCart)
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
                        // Track earliest item for shop ordering
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
                        Items = new List<CartItemResponse>() // Changed from OrderPreviewGroups
                    };
                    
                    // Maintain creation order within shop
                    var sortedShopItems = shopGroup.OrderBy(item => item.CreatedAtUtc);
                    
                    foreach (var item in sortedShopItems)
                    {
                        var product = item.Product;

                        shopCartGroup.Items.Add(new CartItemResponse
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
                        });
                    }

                    shopCartGroup.ShopSubtotal = shopCartGroup.Items.Sum(i => i.Subtotal);
                    response.ShopGroups.Add(shopCartGroup);
                }
            }

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
