using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ConfirmWithdrawalCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid WithdrawalRequestId { get; set; }
}
