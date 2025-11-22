using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ProcessWithdrawalRequestCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid WithdrawalRequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNote { get; set; }
    public IFormFile? ProofImage { get; set; } // Required for approval
}
