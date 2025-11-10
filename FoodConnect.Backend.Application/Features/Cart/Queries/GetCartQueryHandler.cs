using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Cart;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Cart.Queries
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, BaseResponse<CartResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAddressRepository _addressRepository;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IShippingFeeCalculatorService _shippingFeeCalculator;

        public GetCartQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService,
            IAddressRepository addressRepository,
            IDistanceCalculatorService distanceCalculator,
            IShippingFeeCalculatorService shippingFeeCalculator)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
            _addressRepository = addressRepository;
            _distanceCalculator = distanceCalculator;
            _shippingFeeCalculator = shippingFeeCalculator;
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

            var response = await MapCartToResponseAsync(cart, userId);

            return result.BuildSuccess(response, "Get cart successfully");
        }

        private async Task<CartResponse> MapCartToResponseAsync(Domain.Entities.Cart cart, Guid? userId)
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

            // Get buyer's default address for distance/shipping calculations
            Domain.Entities.Address? buyerAddress = null;
            if (userId.HasValue)
            {
                buyerAddress = await _addressRepository.GetDefaultAddressByUserIdAsync(userId.Value);
            }

            if (cart.CartItems != null && cart.CartItems.Any())
            {
                var groupedByShop = cart.CartItems
                    .Where(item => item.Product != null && item.Product.Shop != null)
                    .GroupBy(item => new 
                    { 
                        ShopId = item.Product!.ShopId, 
                        ShopName = item.Product.Shop!.ShopName,
                        ShopStatus = item.Product.Shop.Status,
                        ShopLatitude = item.Product.Shop.Latitude,
                        ShopLongitude = item.Product.Shop.Longitude,
                        ShopCity = item.Product.Shop.City
                    });

                foreach (var shopGroup in groupedByShop)
                {
                    var shopCartGroup = new ShopCartGroup
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        ShopStatus = shopGroup.Key.ShopStatus.ToString(),
                        OrderPreviewGroups = new List<OrderPreviewGroup>()
                    };

                    // Calculate distance to shop if buyer has address
                    double? distanceKm = null;
                    if (buyerAddress != null && 
                        buyerAddress.Latitude.HasValue && 
                        buyerAddress.Longitude.HasValue &&
                        shopGroup.Key.ShopLatitude.HasValue && 
                        shopGroup.Key.ShopLongitude.HasValue)
                    {
                        distanceKm = _distanceCalculator.CalculateDistance(
                            buyerAddress.Latitude.Value,
                            buyerAddress.Longitude.Value,
                            shopGroup.Key.ShopLatitude.Value,
                            shopGroup.Key.ShopLongitude.Value);
                    }

                    // Group items by DeliveryType (Express/Standard)
                    var groupedByDeliveryType = shopGroup.GroupBy(item => 
                        item.Product!.Category?.DeliveryType ?? DeliveryTypeEnum.Standard);

                    foreach (var deliveryGroup in groupedByDeliveryType)
                    {
                        var orderPreviewGroup = new OrderPreviewGroup
                        {
                            DeliveryType = deliveryGroup.Key.ToString(),
                            Items = new List<CartItemResponse>(),
                            DistanceToShopKm = distanceKm
                        };

                        decimal groupSubtotal = 0;

                        foreach (var item in deliveryGroup)
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

                            orderPreviewGroup.Items.Add(cartItem);
                            groupSubtotal += cartItem.Subtotal;
                        }

                        // Calculate shipping fee for this group
                        decimal shippingFee = 0;
                        bool canCheckout = true;
                        var warnings = new List<string>();

                        if (deliveryGroup.Key == DeliveryTypeEnum.Express)
                        {
                            if (distanceKm == null)
                            {
                                canCheckout = false;
                                warnings.Add("Vui lòng thêm địa chỉ mặc định có tọa độ để tính phí giao hàng Express");
                            }
                            else if (distanceKm.Value > ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                            {
                                canCheckout = false;
                                warnings.Add($"Giao hàng Express chỉ khả dụng trong bán kính {ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km (Khoảng cách hiện tại: {distanceKm.Value:F1}km)");
                            }
                            else
                            {
                                shippingFee = (decimal)_shippingFeeCalculator.CalculateShippingFee(distanceKm.Value, DeliveryTypeEnum.Express);
                            }
                        }
                        else // Standard delivery
                        {
                            if (distanceKm == null)
                            {
                                canCheckout = false;
                                warnings.Add("Vui lòng thêm địa chỉ mặc định có tọa độ để tính phí giao hàng");
                            }
                            else
                            {
                                shippingFee = (decimal)_shippingFeeCalculator.CalculateShippingFee(distanceKm.Value, DeliveryTypeEnum.Standard);
                            }
                        }

                        orderPreviewGroup.EstimatedShippingFee = shippingFee;
                        orderPreviewGroup.GroupTotal = groupSubtotal + shippingFee;
                        orderPreviewGroup.CanCheckout = canCheckout;
                        orderPreviewGroup.CheckoutWarnings = warnings;

                        shopCartGroup.OrderPreviewGroups.Add(orderPreviewGroup);
                    }

                    shopCartGroup.ShopSubtotal = shopCartGroup.OrderPreviewGroups
                        .SelectMany(g => g.Items)
                        .Sum(i => i.Subtotal);

                    response.ShopGroups.Add(shopCartGroup);
                }
            }

            // Calculate summary
            var allItems = response.ShopGroups.SelectMany(s => s.OrderPreviewGroups).SelectMany(g => g.Items).ToList();
            var totalShippingFee = response.ShopGroups.SelectMany(s => s.OrderPreviewGroups).Sum(g => g.EstimatedShippingFee);
            var totalAmount = allItems.Sum(i => i.Subtotal);

            response.Summary = new CartSummary
            {
                TotalItems = allItems.Sum(i => i.Quantity),
                TotalAmount = totalAmount,
                TotalOrdersWillBeCreated = response.ShopGroups.Sum(s => s.OrderPreviewGroups.Count),
                TotalShippingFee = totalShippingFee,
                GrandTotal = totalAmount + totalShippingFee
            };

            return response;
        }
    }
}
