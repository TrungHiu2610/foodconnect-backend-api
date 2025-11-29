using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerProductStatisticsQuery : IRequest<BaseResponse<SellerProductStatisticsResponse>>
    {
        public Guid ShopId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TopN { get; set; } = 10;
    }
}
