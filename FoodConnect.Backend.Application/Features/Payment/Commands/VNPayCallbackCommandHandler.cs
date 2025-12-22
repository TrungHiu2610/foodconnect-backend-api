using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Features.Notification.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class VNPayCallbackCommandHandler : IRequestHandler<VNPayCallbackCommand, BaseResponse<PaymentCallbackResult>>
{
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IVNPayService _vnpayService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OrderNotificationService _orderNotificationService;

    public VNPayCallbackCommandHandler(
        IPaymentTransactionRepository paymentRepository,
        IOrderRepository orderRepository,
        IVNPayService vnpayService,
        IUnitOfWork unitOfWork,
        OrderNotificationService orderNotificationService)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _vnpayService = vnpayService;
        _unitOfWork = unitOfWork;
        _orderNotificationService = orderNotificationService;
    }

    public async Task<BaseResponse<PaymentCallbackResult>> Handle(VNPayCallbackCommand request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaymentCallbackResult>();

        var secureHash = request.VnpayData.GetValueOrDefault("vnp_SecureHash", string.Empty);
        if (string.IsNullOrEmpty(secureHash))
        {
            return result.BuildFail("Invalid callback data");
        }

        var isValidSignature = _vnpayService.ValidateSignature(request.VnpayData, secureHash);
        if (!isValidSignature)
        {
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

            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order == null)
            {
                payment.Status = TransactionStatusEnum.Failed;
                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                return result.BuildNotFound("Order not found");
            }

            if (order.Total != (double)callbackResponse.Amount)
            {
                payment.Status = TransactionStatusEnum.Failed;
                _paymentRepository.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                
                return result.BuildFail("Invalid amount");
            }

            order.PaymentStatus = PaymentStatusEnum.Paid;
            
            bool wasAwaitingPayment = order.Status == OrderStatusEnum.AwaitingPayment;
            if (wasAwaitingPayment)
            {
                order.Status = OrderStatusEnum.Pending;
            }
            
            _orderRepository.Update(order);

            _paymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            if (order != null && order.Status == OrderStatusEnum.Pending)
            {
                var fullOrder = await _orderRepository.GetOrderWithDetailsAsync(order.Id);
                if (fullOrder != null)
                {
                    await _orderNotificationService.NotifyNewOrderAsync(fullOrder, cancellationToken);
                }
            }

            return result.BuildSuccess(new PaymentCallbackResult
            {
                Success = true,
                Message = "Payment completed successfully",
                OrderId = payment.OrderId
            }, "Payment completed successfully");
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
