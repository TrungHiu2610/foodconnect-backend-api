using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerOrderStatisticsQuery : IRequest<BaseResponse<SellerOrderStatisticsResponse>>
    {
        public Guid ShopId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public OrderStatusEnum? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
