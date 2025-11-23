using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class VNPayCallbackCommand : IRequest<BaseResponse<PaymentCallbackResult>>
{
    public Dictionary<string, string> VnpayData { get; set; } = new();
}
