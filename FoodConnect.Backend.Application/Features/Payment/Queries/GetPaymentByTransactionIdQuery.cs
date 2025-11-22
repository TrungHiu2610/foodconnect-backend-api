using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Queries;

public class GetPaymentByTransactionIdQuery : IRequest<BaseResponse<PaymentTransactionResponse>>
{
    public string TransactionId { get; set; } = string.Empty;
}
