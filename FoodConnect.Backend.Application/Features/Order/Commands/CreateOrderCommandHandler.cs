using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, BaseResponse<List<OrderDetailDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPromotionUsageRepository _promotionUsageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IShippingFeeCalculatorService _shippingFeeCalculator;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IShopRepository shopRepository,
            IPromotionRepository promotionRepository,
            IPromotionUsageRepository promotionUsageRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService,
            IDistanceCalculatorService distanceCalculator,
            IShippingFeeCalculatorService shippingFeeCalculator)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _promotionRepository = promotionRepository;
            _promotionUsageRepository = promotionUsageRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
            _distanceCalculator = distanceCalculator;
            _shippingFeeCalculator = shippingFeeCalculator;
        }

        public async Task<BaseResponse<List<OrderDetailDto>>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<OrderDetailDto>>();

            // Check if user is authenticated
            if (!_currentUserService.UserId.HasValue)
            {
                return result.BuildUnauthorized("User must be logged in to create an order");
            }

            var buyerId = _currentUserService.UserId.Value;

            // Get cart items
            var cart = await _cartRepository.GetCartWithItemsAsync(buyerId, null);
            if (cart == null || !cart.CartItems.Any())
            {
                return result.BuildFail("Cart is empty");
            }

            // Filter cart items based on request
            var selectedCartItems = cart.CartItems
                .Where(ci => request.CartItemIds.Contains(ci.Id))
                .ToList();

            if (!selectedCartItems.Any())
            {
                return result.BuildFail("No valid cart items found");
            }

            // Validate stock availability before creating orders
            foreach (var cartItem in selectedCartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    return result.BuildFail($"Product not found");
                }

                // Check if product is available
                if (!product.IsAvailable)
                {
                    return result.BuildFail($"Product '{product.Name}' is currently unavailable");
                }

                // Check stock if managed
                if (product.StockQuantity.HasValue && product.StockQuantity.Value < cartItem.Quantity)
                {
                    return result.BuildFail($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Required: {cartItem.Quantity}");
                }
            }

            // Group cart items by shop AND delivery type
            // This will create separate orders for Express and Standard items from same shop
            var orderGroups = selectedCartItems
                .GroupBy(ci => new
                {
                    ShopId = ci.Product!.ShopId,
                    DeliveryType = ci.Product.Category?.DeliveryType ?? DeliveryTypeEnum.Standard
                })
                .ToList();

            // Parse shipping address to get coordinates
            ShippingAddressDto? shippingAddress = null;
            try
            {
                shippingAddress = JsonSerializer.Deserialize<ShippingAddressDto>(request.ShippingAddressJson);
                if (shippingAddress == null)
                {
                    return result.BuildFail("Invalid shipping address format");
                }
            }
            catch (JsonException)
            {
                return result.BuildFail("Failed to parse shipping address");
            }

            // Validate shipping address has coordinates
            if (!shippingAddress.Latitude.HasValue || !shippingAddress.Longitude.HasValue)
            {
                return result.BuildFail("Shipping address must include location coordinates (Latitude and Longitude)");
            }

            // Validate promotion if provided
            Domain.Entities.Promotion? promotion = null;
            if (request.PromotionId.HasValue)
            {
                promotion = await _promotionRepository.GetPromotionWithProductsAsync(request.PromotionId.Value);
                if (promotion == null)
                {
                    return result.BuildFail("Promotion not found");
                }

                // Check if promotion is active
                if (promotion.Status != PromotionStatusEnum.Active)
                {
                    return result.BuildFail($"Promotion is not active (Status: {promotion.Status})");
                }

                // Check date validity
                var now = DateTime.UtcNow;
                if (now < promotion.StartDate || now > promotion.EndDate)
                {
                    return result.BuildFail("Promotion is not valid at this time");
                }

                // Check max usage count
                if (promotion.MaxUsageCount.HasValue && promotion.TotalUsedCount >= promotion.MaxUsageCount.Value)
                {
                    return result.BuildFail("Promotion has reached maximum usage limit");
                }

                // Check user usage limit
                var userUsageCount = await _promotionRepository.GetUserUsageCountAsync(promotion.Id, buyerId);
                if (userUsageCount >= promotion.UsagePerCustomer)
                {
                    return result.BuildFail($"You have already used this promotion {promotion.UsagePerCustomer} time(s)");
                }
            }

            // Pre-validate all Express delivery groups BEFORE creating ANY orders
            // This ensures we fail fast if any Express item is out of range
            foreach (var orderGroup in orderGroups.Where(g => g.Key.DeliveryType == DeliveryTypeEnum.Express))
            {
                var shop = await _shopRepository.GetByIdAsync(orderGroup.Key.ShopId);
                if (shop == null || !shop.Latitude.HasValue || !shop.Longitude.HasValue)
                {
                    return result.BuildFail($"Shop location not configured");
                }

                double distanceKm = _distanceCalculator.CalculateDistance(
                    shippingAddress.Latitude.Value,
                    shippingAddress.Longitude.Value,
                    shop.Latitude.Value,
                    shop.Longitude.Value
                );

                if (distanceKm > (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                {
                    return result.BuildFail($"Express delivery to '{shop.ShopName}' is unavailable. Distance ({distanceKm:F1}km) exceeds maximum ({ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km). Please remove Express items or change delivery address.");
                }
            }

            var createdOrders = new List<Domain.Entities.Order>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var orderGroup in orderGroups)
                {
                    var shopId = orderGroup.Key.ShopId;
                    var deliveryType = orderGroup.Key.DeliveryType;
                    var groupItems = orderGroup.ToList();

                    // Get shop details for distance calculation
                    var shop = await _shopRepository.GetByIdAsync(shopId);
                    if (shop == null)
                    {
                        await transaction.RollbackAsync();
                        return result.BuildFail($"Shop not found");
                    }

                    // Validate shop has coordinates
                    if (!shop.Latitude.HasValue || !shop.Longitude.HasValue)
                    {
                        await transaction.RollbackAsync();
                        return result.BuildFail($"Shop '{shop.ShopName}' does not have location coordinates configured");
                    }

                    // Calculate distance
                    double distanceKm = _distanceCalculator.CalculateDistance(
                        shippingAddress.Latitude.Value,
                        shippingAddress.Longitude.Value,
                        shop.Latitude.Value,
                        shop.Longitude.Value
                    );

                    // Calculate shipping fee based on delivery type
                    // Express: Tiered based on distance
                    // Standard: Distance-based with cap
                    decimal shippingFee = _shippingFeeCalculator.CalculateShippingFee(
                        deliveryType, 
                        distanceKm,
                        shippingAddress.Province,
                        shop.City // Using City as Province
                    );

                    // Calculate order totals
                    double subTotal = (double)groupItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
                    
                    // Calculate promotion discount
                    double discount = 0;
                    decimal? promotionDiscountAmount = null;
                    string? promotionCode = null;
                    Guid? applicablePromotionId = null;

                    if (promotion != null && promotion.ShopId == shopId)
                    {
                        // Check if promotion applies to these products
                        bool isApplicable = promotion.ApplicableToAllProducts;
                        
                        if (!isApplicable)
                        {
                            // Check if any product in this order is part of the promotion
                            var productIds = groupItems.Select(ci => ci.ProductId).ToList();
                            var promotionProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
                            isApplicable = productIds.Any(pid => promotionProductIds.Contains(pid));
                        }

                        if (isApplicable)
                        {
                            // Check minimum order value
                            if ((decimal)subTotal >= promotion.MinimumOrderValue)
                            {
                                // Calculate discount based on type
                                if (promotion.PromotionType == PromotionTypeEnum.Percentage)
                                {
                                    // Percentage discount (DiscountValue is percentage, e.g., 10 for 10%)
                                    promotionDiscountAmount = (decimal)subTotal * (promotion.DiscountValue / 100);
                                }
                                else // FixedAmount
                                {
                                    promotionDiscountAmount = promotion.DiscountValue;
                                }

                                discount = (double)promotionDiscountAmount;
                                applicablePromotionId = promotion.Id;
                                promotionCode = $"PROMO-{promotion.Id.ToString().Substring(0, 8).ToUpper()}";
                            }
                        }
                    }

                    double total = subTotal + ((double)shippingFee) - discount;

                    // Generate order code
                    var orderCode = await GenerateOrderCode();

                    // Get note for this shop (if provided)
                    string? shopNote = null;
                    if (request.OrderNotes != null && request.OrderNotes.TryGetValue(shopId.ToString(), out var note))
                    {
                        shopNote = note;
                    }

                    // Create order
                    var order = new Domain.Entities.Order
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = orderCode,
                        SubTotal = subTotal,
                        ShippingFee = (double)shippingFee,
                        Discount = discount,
                        Total = total,
                        Status = OrderStatusEnum.Pending,
                        PaymentMethod = request.PaymentMethod,
                        DeliveryType = deliveryType, // Auto-determined from Product.Category
                        DistanceKm = Math.Round(distanceKm, 2),
                        ShippingAddressJson = request.ShippingAddressJson,
                        Notes = shopNote, // Use shop-specific note
                        BuyerId = buyerId,
                        ShopId = shopId,
                        PromotionId = applicablePromotionId,
                        PromotionDiscountAmount = promotionDiscountAmount,
                        PromotionCode = promotionCode
                    };

                    await _orderRepository.AddAsync(order);

                    // Create order items
                    foreach (var cartItem in groupItems)
                    {
                        var orderItem = new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = (double)(cartItem.Product?.Price ?? 0),
                            TotalPrice = (double)((cartItem.Product?.Price ?? 0) * cartItem.Quantity)
                        };

                        order.OrderItems.Add(orderItem);
                    }

                    createdOrders.Add(order);

                    // Remove cart items
                    foreach (var cartItem in groupItems)
                    {
                        _cartItemRepository.Remove(cartItem);
                    }

                    // Record promotion usage if applicable
                    if (applicablePromotionId.HasValue)
                    {
                        var promotionUsage = new PromotionUsage
                        {
                            Id = Guid.NewGuid(),
                            PromotionId = applicablePromotionId.Value,
                            UserId = buyerId,
                            OrderId = order.Id,
                            DiscountAmount = promotionDiscountAmount ?? 0,
                            UsedAt = DateTime.UtcNow
                        };
                        await _promotionUsageRepository.AddAsync(promotionUsage);

                        // Update promotion total used count
                        if (promotion != null)
                        {
                            promotion.TotalUsedCount += 1;
                            _promotionRepository.Update(promotion);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                // Load orders with full details and send notifications
                var orderDetails = new List<OrderDetailDto>();
                foreach (var order in createdOrders)
                {
                    var fullOrder = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
                    if (fullOrder != null)
                    {
                        orderDetails.Add(OrderMapper.MapToDetailDto(fullOrder));
                        
                        // Send notification to seller
                        await _orderNotificationService.NotifyNewOrderAsync(fullOrder, cancellationToken);
                    }
                }

                // Build summary message
                var expressCount = createdOrders.Count(o => o.DeliveryType == DeliveryTypeEnum.Express);
                var standardCount = createdOrders.Count(o => o.DeliveryType == DeliveryTypeEnum.Standard);
                var orderSummary = createdOrders.Count == 1 
                    ? "1 order created successfully" 
                    : $"{createdOrders.Count} orders created successfully ({expressCount} Express, {standardCount} Standard)";

                return result.BuildSuccess(orderDetails, orderSummary);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Failed to create order: {ex.Message}");
            }
        }

        private async Task<string> GenerateOrderCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var ordersToday = await _orderRepository.CountOrdersByDateRangeAsync(today, tomorrow);
            var sequentialNumber = (ordersToday + 1).ToString("D5");

            return $"ORD-{datePart}-{sequentialNumber}";
        }
    }
}
