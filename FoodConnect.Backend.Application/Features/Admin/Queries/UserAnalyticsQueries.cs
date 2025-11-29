using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetNewUserStatisticsQuery : IRequest<BaseResponse<NewUserStatisticsResponse>>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupBy { get; set; } = "month"; // day, month, year
    }

    public class GetTopSellersQuery : IRequest<BaseResponse<TopSellersResponse>>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TopN { get; set; } = 10;
    }

    public class GetLoyalCustomersQuery : IRequest<BaseResponse<LoyalCustomersResponse>>
    {
        public int TopN { get; set; } = 10;
    }
}
