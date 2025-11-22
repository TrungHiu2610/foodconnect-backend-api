using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ResolveWithdrawalIssueCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid WithdrawalRequestId { get; set; }
    public string Resolution { get; set; } = string.Empty; // "Resolved" hoặc "NeedsReinvestigation"
    public string AdminNote { get; set; } = string.Empty;
    public IFormFile? NewProofImage { get; set; } // Optional: Upload proof mới nếu cần
}
