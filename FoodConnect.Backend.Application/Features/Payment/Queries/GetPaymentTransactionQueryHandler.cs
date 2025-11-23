using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Queries;

public class GetPaymentTransactionQueryHandler : IRequestHandler<GetPaymentTransactionQuery, BaseResponse<PaymentTransactionResponse>>
{
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetPaymentTransactionQueryHandler(
        IPaymentTransactionRepository paymentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaymentTransactionResponse>> Handle(GetPaymentTransactionQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaymentTransactionResponse>();

        var userId = _currentUserService.UserId;
        if (userId == null)
            return result.BuildUnauthorized();

        var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);
        if (payment == null)
            return result.BuildNotFound("Payment transaction not found");

        if (payment.BuyerId != userId.Value)
            return result.BuildForbidden("You can only view your own payment transactions");

        var response = _mapper.Map<PaymentTransactionResponse>(payment);
        return result.BuildSuccess(response, "Payment transaction retrieved successfully");
    }
}
