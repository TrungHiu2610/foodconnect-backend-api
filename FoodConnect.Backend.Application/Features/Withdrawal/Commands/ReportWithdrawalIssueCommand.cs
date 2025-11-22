using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Commands;

public class ReportWithdrawalIssueCommand : IRequest<BaseResponse<CreateOrUpdateResponse>>
{
    public Guid WithdrawalRequestId { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public IFormFile? IssueImage { get; set; }
}
