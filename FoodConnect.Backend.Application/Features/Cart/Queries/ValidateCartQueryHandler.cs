using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class ValidateCartQueryHandler : IRequestHandler<ValidateCartQuery, BaseResponse<CartValidationResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public ValidateCartQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CartValidationResponse>> Handle(ValidateCartQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CartValidationResponse>();
            var validationResult = new CartValidationResponse
            {
                IsValid = true,
                Errors = new List<CartValidationError>(),
                Warnings = new List<CartValidationWarning>()
            };

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
                return result.BuildFail("Cart not found", 404);
            }

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return result.BuildFail("Cart is empty", 400);
            }

            // Filter items to validate
            var itemsToValidate = cart.CartItems;
            if (request.CartItemIds != null && request.CartItemIds.Any())
            {
                itemsToValidate = cart.CartItems
                    .Where(i => request.CartItemIds.Contains(i.Id))
                    .ToList();
            }

            foreach (var cartItem in itemsToValidate)
            {
                var product = cartItem.Product;
                var shop = product?.Shop;

                // Validate product exists
                if (product == null)
                {
                    validationResult.Errors.Add(new CartValidationError
                    {
                        CartItemId = cartItem.Id,
                        ProductName = "Unknown Product",
                        ErrorType = "ProductNotFound",
                        Message = "Product no longer exists"
                    });
                    validationResult.IsValid = false;
                    continue;
                }

                // Validate product is active
                if (product.Status != ProductStatusEnum.Active)
                {
                    validationResult.Errors.Add(new CartValidationError
                    {
                        CartItemId = cartItem.Id,
                        ProductName = product.Name,
                        ErrorType = "ProductInactive",
                        Message = $"Product '{product.Name}' is no longer available"
                    });
                    validationResult.IsValid = false;
                }

                // Validate product availability (manual or stock-based)
                if (!product.IsAvailable)
                {
                    validationResult.Errors.Add(new CartValidationError
                    {
                        CartItemId = cartItem.Id,
                        ProductName = product.Name,
                        ErrorType = "ProductUnavailable",
                        Message = $"Product '{product.Name}' is currently unavailable"
                    });
                    validationResult.IsValid = false;
                }

                // Validate shop exists and is active
                if (shop == null)
                {
                    validationResult.Errors.Add(new CartValidationError
                    {
                        CartItemId = cartItem.Id,
                        ProductName = product.Name,
                        ErrorType = "ShopNotFound",
                        Message = "Shop no longer exists"
                    });
                    validationResult.IsValid = false;
                    continue;
                }

                if (shop.Status != ShopStatusEnum.Active)
                {
                    validationResult.Errors.Add(new CartValidationError
                    {
                        CartItemId = cartItem.Id,
                        ProductName = product.Name,
                        ErrorType = "ShopClosed",
                        Message = $"Shop '{shop.ShopName}' is temporarily closed",
                        Details = new { ShopStatus = shop.Status.ToString() }
                    });
                    validationResult.IsValid = false;
                }

                // Validate stock availability (only for Standard delivery products with stock tracking)
                // Skip stock validation for Express delivery products
                var category = product.Category;
                if (product.StockQuantity.HasValue && category?.DeliveryType != DeliveryTypeEnum.Express)
                {
                    var availableStock = product.StockQuantity.Value;

                    if (availableStock == 0)
                    {
                        validationResult.Errors.Add(new CartValidationError
                        {
                            CartItemId = cartItem.Id,
                            ProductName = product.Name,
                            ErrorType = "OutOfStock",
                            Message = $"Product '{product.Name}' is out of stock",
                            Details = new { AvailableStock = 0, RequestedQuantity = cartItem.Quantity }
                        });
                        validationResult.IsValid = false;
                    }
                    else if (availableStock < cartItem.Quantity)
                    {
                        validationResult.Errors.Add(new CartValidationError
                        {
                            CartItemId = cartItem.Id,
                            ProductName = product.Name,
                            ErrorType = "InsufficientStock",
                            Message = $"Only {availableStock} {product.Unit} available for '{product.Name}'",
                            Details = new { AvailableStock = availableStock, RequestedQuantity = cartItem.Quantity }
                        });
                        validationResult.IsValid = false;
                    }
                    else if (availableStock < cartItem.Quantity * 1.5) // Low stock warning
                    {
                        validationResult.Warnings.Add(new CartValidationWarning
                        {
                            CartItemId = cartItem.Id,
                            ProductName = product.Name,
                            WarningType = "LowStock",
                            Message = $"Only {availableStock} {product.Unit} left for '{product.Name}'"
                        });
                    }
                }

                // Note: Price validation would require storing original price in CartItem
                // For now, we assume current price is correct
            }

            return result.BuildSuccess(validationResult, 
                validationResult.IsValid ? "Cart is valid" : "Cart validation failed");
        }
    }
}
