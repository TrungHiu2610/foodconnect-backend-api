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
    public class GetCheckoutPreviewQueryHandler : IRequestHandler<GetCheckoutPreviewQuery, BaseResponse<CheckoutPreviewResponse>>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAddressRepository _addressRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IShippingFeeCalculatorService _shippingFeeCalculator;

        public GetCheckoutPreviewQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService,
            IAddressRepository addressRepository,
            IShopRepository shopRepository,
            IDistanceCalculatorService distanceCalculator,
            IShippingFeeCalculatorService shippingFeeCalculator)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
            _addressRepository = addressRepository;
            _shopRepository = shopRepository;
            _distanceCalculator = distanceCalculator;
            _shippingFeeCalculator = shippingFeeCalculator;
        }

        public async Task<BaseResponse<CheckoutPreviewResponse>> Handle(GetCheckoutPreviewQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CheckoutPreviewResponse>();

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
                return result.BuildFail("Cart not found");
            }

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return result.BuildSuccess(new CheckoutPreviewResponse
                {
                    CartId = Guid.Empty,
                    ShopGroups = new List<CheckoutShopGroup>(),
                    Summary = new CheckoutSummary()
                }, "Empty cart");
            }

            var response = await MapCartToCheckoutPreviewAsync(cart, userId, request.CartItemIds);

            return result.BuildSuccess(response, "Get checkout preview successfully");
        }

        private async Task<CheckoutPreviewResponse> MapCartToCheckoutPreviewAsync(
            Domain.Entities.Cart cart,
            Guid? userId,
            List<Guid>? selectedCartItemIds)
        {
            var response = new CheckoutPreviewResponse
            {
                CartId = cart.Id,
                ShopGroups = new List<CheckoutShopGroup>()
            };

            Domain.Entities.Address? buyerAddress = null;
            if (userId.HasValue)
            {
                buyerAddress = await _addressRepository.GetDefaultAddressByUserIdAsync(userId.Value);
            }

            if (cart.CartItems != null && cart.CartItems.Any())
            {
                var itemsToCheckout = cart.CartItems;
                if (selectedCartItemIds != null && selectedCartItemIds.Any())
                {
                    itemsToCheckout = cart.CartItems
                        .Where(i => selectedCartItemIds.Contains(i.Id))
                        .ToList();
                }

                var sortedCartItems = itemsToCheckout
                    .Where(item => item.Product != null && item.Product.Shop != null)
                    .OrderBy(item => item.CreatedAtUtc)
                    .ToList();

                var groupedByShop = sortedCartItems
                    .GroupBy(item => new
                    {
                        ShopId = item.Product!.ShopId,
                        ShopName = item.Product.Shop!.ShopName,
                        ShopStatus = item.Product.Shop.Status,
                        ShopLatitude = item.Product.Shop.Latitude,
                        ShopLongitude = item.Product.Shop.Longitude,
                        ShopCity = item.Product.Shop.City,
                        FirstItemCreatedAt = sortedCartItems
                            .Where(ci => ci.Product!.ShopId == item.Product!.ShopId)
                            .Min(ci => ci.CreatedAtUtc)
                    })
                    .OrderBy(g => g.Key.FirstItemCreatedAt);

                foreach (var shopGroup in groupedByShop)
                {
                    var shopCartGroup = new CheckoutShopGroup
                    {
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        ShopStatus = shopGroup.Key.ShopStatus.ToString(),
                        OrderPreviewGroups = new List<OrderPreviewGroup>()
                    };

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

                    var groupedByDeliveryType = shopGroup
                        .GroupBy(item => item.Product!.Category?.DeliveryType ?? DeliveryTypeEnum.Standard)
                        .OrderBy(g => g.Key); // Express first, then Standard

                    foreach (var deliveryGroup in groupedByDeliveryType)
                    {
                        var orderPreviewGroup = new OrderPreviewGroup
                        {
                            DeliveryType = deliveryGroup.Key.ToString(),
                            Items = new List<CartItemResponse>(),
                            DistanceToShopKm = distanceKm
                        };

                        decimal groupSubtotal = 0;

                        var sortedDeliveryItems = deliveryGroup.OrderBy(item => item.CreatedAtUtc);

                        foreach (var item in sortedDeliveryItems)
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

                        decimal shippingFee = 0;
                        bool groupCanCheckout = true;
                        var warnings = new List<string>();

                        if (deliveryGroup.Key == DeliveryTypeEnum.Express)
                        {
                            if (distanceKm == null)
                            {
                                groupCanCheckout = false;
                                warnings.Add("Vui lòng thêm địa chỉ mặc định có tọa độ để tính phí giao hàng Express");
                            }
                            else if (distanceKm.Value > (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                            {
                                groupCanCheckout = false;
                                warnings.Add($"Giao hàng Express chỉ khả dụng trong bán kính {ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km (Khoảng cách hiện tại: {distanceKm.Value:F1}km)");
                            }
                            else
                            {
                                shippingFee = _shippingFeeCalculator.CalculateShippingFee(DeliveryTypeEnum.Express, distanceKm.Value);
                            }
                        }
                        else // Standard delivery
                        {
                            if (distanceKm == null)
                            {
                                groupCanCheckout = false;
                                warnings.Add("Vui lòng thêm địa chỉ mặc định có tọa độ để tính phí giao hàng");
                            }
                            else
                            {
                                var shop = await _shopRepository.GetByIdAsync(shopGroup.Key.ShopId);
                                shippingFee = _shippingFeeCalculator.CalculateShippingFee(
                                    DeliveryTypeEnum.Standard,
                                    distanceKm.Value,
                                    buyerAddress!.City,
                                    shop!.City);
                            }
                        }

                        orderPreviewGroup.EstimatedShippingFee = shippingFee;
                        orderPreviewGroup.GroupTotal = groupSubtotal + shippingFee;
                        orderPreviewGroup.CanCheckout = groupCanCheckout;
                        orderPreviewGroup.CheckoutWarnings = warnings;

                        shopCartGroup.OrderPreviewGroups.Add(orderPreviewGroup);
                    }

                    shopCartGroup.ShopSubtotal = shopCartGroup.OrderPreviewGroups
                        .SelectMany(g => g.Items)
                        .Sum(i => i.Subtotal);
                    shopCartGroup.ShopTotalShipping = shopCartGroup.OrderPreviewGroups
                        .Sum(g => g.EstimatedShippingFee);
                    shopCartGroup.ShopGrandTotal = shopCartGroup.ShopSubtotal + shopCartGroup.ShopTotalShipping;

                    response.ShopGroups.Add(shopCartGroup);
                }
            }

            var allItems = response.ShopGroups
                .SelectMany(s => s.OrderPreviewGroups)
                .SelectMany(g => g.Items)
                .ToList();

            var allOrderGroups = response.ShopGroups
                .SelectMany(s => s.OrderPreviewGroups)
                .ToList();

            var totalShippingFee = allOrderGroups.Sum(g => g.EstimatedShippingFee);
            var totalAmount = allItems.Sum(i => i.Subtotal);
            var canCheckout = allOrderGroups.All(g => g.CanCheckout);
            var checkoutBlockers = allOrderGroups
                .Where(g => !g.CanCheckout)
                .SelectMany(g => g.CheckoutWarnings)
                .Distinct()
                .ToList();

            response.Summary = new CheckoutSummary
            {
                TotalItems = allItems.Count,
                TotalAmount = totalAmount,
                TotalOrdersWillBeCreated = allOrderGroups.Count,
                TotalShippingFee = totalShippingFee,
                GrandTotal = totalAmount + totalShippingFee,
                CanCheckout = canCheckout,
                CheckoutBlockers = checkoutBlockers
            };

            return response;
        }

    }
}
