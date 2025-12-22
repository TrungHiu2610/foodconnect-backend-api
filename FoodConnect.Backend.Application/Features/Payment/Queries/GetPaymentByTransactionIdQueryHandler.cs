using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Queries;

public class GetPaymentByTransactionIdQueryHandler : IRequestHandler<GetPaymentByTransactionIdQuery, BaseResponse<PaymentTransactionResponse>>
{
    private readonly IPaymentTransactionRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentByTransactionIdQueryHandler(
        IPaymentTransactionRepository paymentRepository,
        IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PaymentTransactionResponse>> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
    {
        var result = new BaseResponse<PaymentTransactionResponse>();

        var payment = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId);

        if (payment == null)
            return result.BuildNotFound("Payment transaction not found");

        var response = _mapper.Map<PaymentTransactionResponse>(payment);
        return result.BuildSuccess(response, "Success");
    }
}
