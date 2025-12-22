using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetFlaggedReviewsQuery : IRequest<BaseResponse<GetFlaggedReviewsResponse>>
    {
        public ReviewStatusEnum? Status { get; set; } // Toxic hoặc Spam
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetFlaggedReviewsResponse
    {
        public List<FlaggedReviewDto> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class FlaggedReviewDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public ReviewStatusEnum Status { get; set; }
        public ReviewRejectionReasonEnum? RejectionReason { get; set; }
        public string? RejectionDetails { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ModeratedAt { get; set; }
    }
}
