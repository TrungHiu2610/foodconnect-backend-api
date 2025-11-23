using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetOrderStatusOverviewQuery : IRequest<BaseResponse<OrderStatusOverviewResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetCancellationRateQuery : IRequest<BaseResponse<CancellationRateResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class GetTopProductsQuery : IRequest<BaseResponse<TopProductsResponse>>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TopN { get; set; } = 10;
        public Guid? CategoryId { get; set; }
    }
}
