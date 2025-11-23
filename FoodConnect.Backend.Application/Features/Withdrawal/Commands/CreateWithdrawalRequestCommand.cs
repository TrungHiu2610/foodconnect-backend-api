using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class CreateWithdrawalRequestCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public decimal RequestedAmount { get; set; }
    public int PaymentMethod { get; set; }
    public string PaymentAccountNumber { get; set; } = string.Empty;
    public string PaymentAccountName { get; set; } = string.Empty;
    public string? SellerNote { get; set; }
}
