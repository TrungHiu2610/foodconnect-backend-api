using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;
using FoodConnect.Backend.Application.Commons.Models;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Withdrawal.Queries;

public class GetWithdrawalRequestsQuery : IRequest<BaseResponse<PaginatedList<WithdrawalRequestListResponse>>>
{
    public int? Status { get; set; }
    public Guid? SellerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
