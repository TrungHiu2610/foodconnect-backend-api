using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Application.Features.Order.Mappers;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, BaseResponse<List<OrderDetailDto>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly OrderNotificationService _orderNotificationService;
        private const double DEFAULT_SHIPPING_FEE = 15000; // 15,000 VND per order

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            OrderNotificationService orderNotificationService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _orderNotificationService = orderNotificationService;
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

            // Group cart items by shop
            var itemsByShop = selectedCartItems
                .GroupBy(ci => ci.Product!.ShopId)
                .ToList();

            var createdOrders = new List<Domain.Entities.Order>();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var shopGroup in itemsByShop)
                {
                    var shopId = shopGroup.Key;
                    var shopItems = shopGroup.ToList();

                    // Calculate order totals
                    double subTotal = (double)shopItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity);
                    double shippingFee = DEFAULT_SHIPPING_FEE;
                    double discount = 0; // TODO: Apply discounts in future
                    double total = subTotal + shippingFee - discount;

                    // Generate order code
                    var orderCode = await GenerateOrderCode();

                    // Create order
                    var order = new Domain.Entities.Order
                    {
                        Id = Guid.NewGuid(),
                        OrderCode = orderCode,
                        SubTotal = subTotal,
                        ShippingFee = shippingFee,
                        Discount = discount,
                        Total = total,
                        Status = OrderStatusEnum.Pending,
                        PaymentMethod = request.PaymentMethod,
                        ShippingAddressJson = request.ShippingAddressJson,
                        Notes = request.Notes,
                        BuyerId = buyerId,
                        ShopId = shopId
                    };

                    await _orderRepository.AddAsync(order);

                    // Create order items
                    foreach (var cartItem in shopItems)
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
                    foreach (var cartItem in shopItems)
                    {
                        _cartItemRepository.Remove(cartItem);
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

                return result.BuildSuccess(orderDetails, "Orders created successfully");
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
