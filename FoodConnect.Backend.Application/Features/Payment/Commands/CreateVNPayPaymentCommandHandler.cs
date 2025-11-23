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

        var order = await _orderRepository.GetOrderWithDetailsAsync(request.OrderId);
        if (order == null)
            return result.BuildNotFound("Order not found");

        if (order.BuyerId != userId.Value)
            return result.BuildForbidden("You can only pay for your own orders");

        if (order.PaymentMethod != PaymentMethodEnum.VNPay)
            return result.BuildFail("This order is not configured for VNPay payment");

        if (order.PaymentStatus == PaymentStatusEnum.Paid)
            return result.BuildConflict("This order has already been paid");

        if (order.Status == OrderStatusEnum.Cancelled)
            return result.BuildFail("Cannot pay for cancelled orders");

        var existingPayment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
        if (existingPayment != null && existingPayment.Status == TransactionStatusEnum.Completed)
            return result.BuildConflict("Payment already completed for this order");

        var paymentRequest = new VNPayCreatePaymentRequest
        {
            OrderId = order.Id,
            Amount = (decimal)order.Total,
            OrderInfo = $"Order {order.OrderCode}", 
            IpAddress = request.IpAddress
        };

        var paymentUrlResponse = await _vnpayService.CreatePaymentUrlAsync(paymentRequest);

        var paymentTransaction = new PaymentTransaction
        {
            OrderId = order.Id,
            BuyerId = userId.Value,
            Amount = (decimal)order.Total,
            TransactionId = paymentUrlResponse.TransactionId,
            Status = TransactionStatusEnum.Pending,
            PaymentMethod = "VNPay"
        };

        await _paymentRepository.AddAsync(paymentTransaction);
        await _unitOfWork.SaveChangesAsync();

        return result.BuildSuccess(paymentUrlResponse.PaymentUrl, "Payment URL created successfully");
    }
}
