using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Payment.Commands;

public class CreateVNPayPaymentCommand : IRequest<BaseResponse<string>>
{
    public Guid OrderId { get; set; } // Kept for backward compatibility (primary order)
    public List<Guid>? OrderIds { get; set; } // For multi-order payments
    public string IpAddress { get; set; } = string.Empty;
    public string Platform { get; set; } = "web";
}
