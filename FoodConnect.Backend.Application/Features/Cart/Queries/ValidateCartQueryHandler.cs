using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class ValidateCartQueryHandler : IRequestHandler<ValidateCartQuery, BaseResponse<CartValidationResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDistanceCalculatorService _distanceCalculator;

        public ValidateCartQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService,
            IDistanceCalculatorService distanceCalculator)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
            _distanceCalculator = distanceCalculator;
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
                            Message = $"Only {availableStock} items available for '{product.Name}'",
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
                            Message = $"Only {availableStock} items left for '{product.Name}'"
                        });
                    }
                }


                if (category != null && category.DeliveryType == DeliveryTypeEnum.Express)
                {
                    bool buyerHasLocation = request.BuyerLatitude.HasValue && request.BuyerLongitude.HasValue;

                    if (!buyerHasLocation)
                    {
                        validationResult.Warnings.Add(new CartValidationWarning
                        {
                            CartItemId = cartItem.Id,
                            ProductName = product.Name,
                            WarningType = "LocationRequired",
                            Message = $"Bạn cần bật vị trí để đặt món Express: '{product.Name}'"
                        });
                    }
                    else if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                    {
                        double distance = _distanceCalculator.CalculateDistance(
                            request.BuyerLatitude.Value,
                            request.BuyerLongitude.Value,
                            shop.Latitude.Value,
                            shop.Longitude.Value
                        );

                        if (distance > (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                        {
                            validationResult.Warnings.Add(new CartValidationWarning
                            {
                                CartItemId = cartItem.Id,
                                ProductName = product.Name,
                                WarningType = "OutsideDeliveryRange",
                                Message = $"Sản phẩm '{product.Name}' nằm ngoài phạm vi giao hàng Express ({distance:F2}km > {ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km)"
                            });
                        }
                    }
                    else
                    {
                        validationResult.Warnings.Add(new CartValidationWarning
                        {
                            CartItemId = cartItem.Id,
                            ProductName = product.Name,
                            WarningType = "ShopLocationUnavailable",
                            Message = $"Cửa hàng của sản phẩm '{product.Name}' chưa cập nhật vị trí, không thể giao Express"
                        });
                    }
                }
            }

            return result.BuildSuccess(validationResult, 
                validationResult.IsValid ? "Cart is valid" : "Cart validation failed");
        }
    }
}
