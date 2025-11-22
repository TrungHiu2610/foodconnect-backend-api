using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Queries;

public class GetPaymentTransactionQuery : IRequest<BaseResponse<PaymentTransactionResponse>>
{
    public Guid OrderId { get; set; }
}
