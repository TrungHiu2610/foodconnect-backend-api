using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetMyWithdrawalDetailQuery : IRequest<BaseResponse<WithdrawalRequestResponse>>
{
    public Guid RequestId { get; set; }
}
