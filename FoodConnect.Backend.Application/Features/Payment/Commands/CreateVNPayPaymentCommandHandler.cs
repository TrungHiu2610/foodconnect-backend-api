using FoodConnect.Backend.Application.Commons.DTOs.Requests.Payment;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class CreateVNPayPaymentCommandHandler : IRequestHandler<CreateVNPayPaymentCommand, BaseResponse<string>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVNPayService _vnpayService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVNPayPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentTransactionRepository paymentRepository,
        ICurrentUserService currentUserService,
        IVNPayService vnpayService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _vnpayService = vnpayService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<string>> Handle(CreateVNPayPaymentCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<string>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        // Support multi-order payment
        var orderIds = request.OrderIds?.Any() == true ? request.OrderIds : new List<Guid> { request.OrderId };
        
        var orders = new List<Domain.Entities.Order>();
        decimal totalAmount = 0;
        
        foreach (var orderId in orderIds)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
                return result.BuildNotFound($"Order {orderId} not found");

            if (order.BuyerId != userId.Value)
                return result.BuildForbidden("You can only pay for your own orders");

            if (order.PaymentMethod != PaymentMethodEnum.VNPay)
                return result.BuildFail($"Order {order.OrderCode} is not configured for VNPay payment");

            if (order.PaymentStatus == PaymentStatusEnum.Paid)
                return result.BuildFail($"Order {order.OrderCode} has already been paid");

            if (order.Status == OrderStatusEnum.Cancelled)
                return result.BuildFail($"Cannot pay for cancelled order {order.OrderCode}");

            orders.Add(order);
            totalAmount += (decimal)order.Total;
        }

        // Check if payment already exists for any order
        foreach (var orderId in orderIds)
        {
            var existingPayment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (existingPayment != null && existingPayment.Status == TransactionStatusEnum.Completed)
                return result.BuildConflict($"Payment already completed for one or more orders");
        }

        var primaryOrder = orders.First();
        var orderInfo = orders.Count == 1 
            ? $"Order {primaryOrder.OrderCode}"
            : $"Payment for {orders.Count} orders - Total {totalAmount} VND";

        var paymentRequest = new VNPayCreatePaymentRequest
        {
            OrderId = primaryOrder.Id,
            Amount = totalAmount,
            OrderInfo = orderInfo,
            IpAddress = request.IpAddress,
            Platform = request.Platform
        };

        var paymentUrlResponse = await _vnpayService.CreatePaymentUrlAsync(paymentRequest);

        var paymentTransaction = new PaymentTransaction
        {
            OrderId = primaryOrder.Id,
            BuyerId = userId.Value,
            Amount = totalAmount,
            TransactionId = paymentUrlResponse.TransactionId,
            Status = TransactionStatusEnum.Pending,
            PaymentMethod = "VNPay",
            OrderIds = orders.Count > 1 ? System.Text.Json.JsonSerializer.Serialize(orderIds) : null
        };

        await _paymentRepository.AddAsync(paymentTransaction);
        await _unitOfWork.SaveChangesAsync();

        return result.BuildSuccess(paymentUrlResponse.PaymentUrl, "Payment URL created successfully");
    }
}
