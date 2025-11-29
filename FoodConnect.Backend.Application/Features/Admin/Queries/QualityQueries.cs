using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetComplaintStatisticsQuery : IRequest<BaseResponse<ComplaintStatisticsResponse>>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetSellerHealthScoreQuery : IRequest<BaseResponse<List<SellerHealthScoreResponse>>>
    {
        public Guid? ShopId { get; set; }
        public int TopN { get; set; } = 20;
    }

    public class GetBuyerRiskScoreQuery : IRequest<BaseResponse<List<BuyerRiskScoreResponse>>>
    {
        public Guid? BuyerId { get; set; }
        public int TopN { get; set; } = 20;
    }
}
