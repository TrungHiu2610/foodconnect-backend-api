using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
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

            // Get or create cart
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
                return result.BuildFail("Either user must be logged in or session ID must be provided");
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
                        UserId = userId ?? Guid.Empty,
                        SessionId = request.SessionId ?? string.Empty,
                        CartItems = new List<CartItem>()
                    };
                    await _cartRepository.AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Check if product already in cart
                var existingItem = await _cartItemRepository.GetCartItemAsync(cart.Id, request.ProductId);

                if (existingItem != null)
                {
                    // Update quantity
                    existingItem.Quantity += request.Quantity;
                    _cartItemRepository.Update(existingItem);
                }
                else
                {
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
                Items = new List<CartItemResponse>()
            };

            if (cart.CartItems != null)
            {
                foreach (var item in cart.CartItems)
                {
                    response.Items.Add(new CartItemResponse
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? string.Empty,
                        ProductThumbnail = item.Product?.ProductAssets?.FirstOrDefault(a => a.IsThumbnail)?.AssetUrl,
                        ProductPrice = item.Product?.Price ?? 0,
                        ProductUnit = item.Product?.Unit ?? string.Empty,
                        Quantity = item.Quantity,
                        Subtotal = (item.Product?.Price ?? 0) * item.Quantity,
                        ShopId = item.Product?.ShopId ?? Guid.Empty,
                        ShopName = item.Product?.Shop?.Name ?? string.Empty
                    });
                }
            }

            response.TotalItems = response.Items.Sum(i => i.Quantity);
            response.TotalAmount = response.Items.Sum(i => i.Subtotal);

            return response;
        }
    }
}
