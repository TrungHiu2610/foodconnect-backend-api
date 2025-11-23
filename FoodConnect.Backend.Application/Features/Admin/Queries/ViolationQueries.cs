using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    // Query 1: Get account list with violation scores
    public class GetAccountViolationListQuery : IRequest<BaseResponse<List<AccountViolationListResponse>>>
    {
        public string? Role { get; set; } // "Seller" or "Buyer" to filter
        public decimal? MinScore { get; set; } // Filter by min health/risk score
        public decimal? MaxScore { get; set; } // Filter by max health/risk score
        public bool? HasWarningBadge { get; set; } // Filter only accounts with warning
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "Score"; // "Score", "Name", "CreatedAt"
        public string? SortOrder { get; set; } = "desc"; // "asc" or "desc"
    }

    // Query 2: Get account activity details
    public class GetAccountActivityDetailQuery : IRequest<BaseResponse<AccountActivityDetailResponse>>
    {
        public Guid UserId { get; set; }
        public int RecentOrdersLimit { get; set; } = 20;
    }
}
