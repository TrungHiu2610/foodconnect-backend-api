using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetSystemRevenueQuery : IRequest<BaseResponse<SystemRevenueResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? SellerId { get; set; }
    }

    public class GetRevenueBySellerQuery : IRequest<BaseResponse<RevenueBySellerResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TopN { get; set; } = 20;
    }

    public class GetRefundStatisticsQuery : IRequest<BaseResponse<RefundStatisticsResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? SellerId { get; set; }
    }
}
