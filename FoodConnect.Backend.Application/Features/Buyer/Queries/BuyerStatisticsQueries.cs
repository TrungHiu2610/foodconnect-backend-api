using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Buyer;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Buyer.Queries
{
    public class GetBuyerSpendingStatisticsQuery : IRequest<BaseResponse<BuyerSpendingStatisticsResponse>>
    {
        public Guid BuyerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetBuyerOrderActivityQuery : IRequest<BaseResponse<BuyerOrderActivityResponse>>
    {
        public Guid BuyerId { get; set; }
        public int TopProductsCount { get; set; } = 10;
    }
}
