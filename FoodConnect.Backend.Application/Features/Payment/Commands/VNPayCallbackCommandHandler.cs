using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class VNPayCallbackCommandHandler : IRequestHandler<VNPayCallbackCommand, BaseResponse<PaymentCallbackResult>>
{
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVNPayService _vnpayService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderNotificationService _orderNotificationService;
    private readonly ILogger<VNPayCallbackCommandHandler> _logger;

    public VNPayCallbackCommandHandler(
        IPaymentTransactionRepository paymentRepository,
        IOrderRepository orderRepository,
        IVNPayService vnpayService,
        IUnitOfWork unitOfWork,
        OrderNotificationService orderNotificationService,
        ILogger<VNPayCallbackCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _vnpayService = vnpayService;
        _unitOfWork = unitOfWork;
        _orderNotificationService = orderNotificationService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaymentCallbackResult>> Handle(VNPayCallbackCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaymentCallbackResult>();

        // Log all callback data for debugging
        _logger.LogInformation("=== VNPay Callback Debug Info ===");
        _logger.LogInformation("Callback Data Count: {Count}", request.VnpayData.Count);
        foreach (var kvp in request.VnpayData.OrderBy(x => x.Key))
        {
            _logger.LogInformation("  {Key} = {Value}", kvp.Key, kvp.Value);
        }

        var secureHash = request.VnpayData.GetValueOrDefault("vnp_SecureHash", string.Empty);
        if (string.IsNullOrEmpty(secureHash))
        {
            _logger.LogError("VNPay callback missing vnp_SecureHash");
            return result.BuildFail("Invalid callback data");
        }

        _logger.LogInformation("Received SecureHash: {Hash}", secureHash);

        var isValidSignature = _vnpayService.ValidateSignature(request.VnpayData, secureHash);
        if (!isValidSignature)
        {
            _logger.LogError("VNPay signature validation failed");
            return result.BuildFail("Invalid signature");
        }

        var callbackResponse = _vnpayService.ProcessCallback(request.VnpayData);

        var transactionId = callbackResponse.TransactionId;
        var payment = await _paymentRepository.GetByTransactionIdAsync(transactionId);

        if (payment == null)
        {
            return result.BuildNotFound("Payment transaction not found");
        }

        if (payment.Status == TransactionStatusEnum.Completed)
        {
            return result.BuildSuccess(new PaymentCallbackResult
            {
                Success = true,
                Message = "Order already confirmed",
                OrderId = payment.OrderId
            }, "Order already confirmed");
        }

        payment.VnpayData = JsonSerializer.Serialize(request.VnpayData);
        payment.CompletedAt = DateTime.UtcNow;

        if (callbackResponse.Success)
        {
            payment.Status = TransactionStatusEnum.Completed;

            // Handle multi-order payment
            var orderIds = new List<Guid>();
            if (!string.IsNullOrEmpty(payment.OrderIds))
            {
                try
                {
                    orderIds = JsonSerializer.Deserialize<List<Guid>>(payment.OrderIds) ?? new List<Guid>();
                }
                catch
                {
                    orderIds = new List<Guid> { payment.OrderId };
                }
            }
            else
            {
                orderIds = new List<Guid> { payment.OrderId };
            }

            // Validate total amount matches sum of all orders
            decimal totalOrderAmount = 0;
            var orders = new List<Domain.Entities.Order>();
            
            foreach (var orderId in orderIds)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    payment.Status = TransactionStatusEnum.Failed;
                    _paymentRepository.Update(payment);
                    await _unitOfWork.SaveChangesAsync();
                    return result.BuildNotFound($"Order {orderId} not found");
                }
                orders.Add(order);
                totalOrderAmount += (decimal)order.Total;
            }

            // Validate payment amount
            if (totalOrderAmount != callbackResponse.Amount)
            {
                payment.Status = TransactionStatusEnum.Failed;
                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                
                return result.BuildFail($"Invalid amount. Expected: {totalOrderAmount}, Received: {callbackResponse.Amount}");
            }

            // Update all orders
            foreach (var order in orders)
            {
                order.PaymentStatus = PaymentStatusEnum.Paid;
                
                bool wasAwaitingPayment = order.Status == OrderStatusEnum.AwaitingPayment;
                if (wasAwaitingPayment)
                {
                    order.Status = OrderStatusEnum.Pending;
                }
                
                _orderRepository.Update(order);
            }

            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            // Send notifications for all orders
            foreach (var order in orders)
            {
                if (order.Status == OrderStatusEnum.Pending)
                {
                    var fullOrder = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
                    if (fullOrder != null)
                    {
                        await _orderNotificationService.NotifyNewOrderAsync(fullOrder, cancellationToken);
                    }
                }
            }

            var successMessage = orders.Count == 1
                ? "Payment completed successfully"
                : $"Payment completed successfully for {orders.Count} orders";

            return result.BuildSuccess(new PaymentCallbackResult
            {
                Success = true,
                Message = successMessage,
                OrderId = payment.OrderId
            }, successMessage);
        }
        else
        {
            payment.Status = TransactionStatusEnum.Failed;
            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return result.BuildFail($"Payment failed: {callbackResponse.Message}");
        }
    }
}
