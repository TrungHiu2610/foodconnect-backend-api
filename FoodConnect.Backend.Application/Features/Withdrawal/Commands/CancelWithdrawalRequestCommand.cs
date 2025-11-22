using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class CancelWithdrawalRequestCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid WithdrawalRequestId { get; set; }
}
