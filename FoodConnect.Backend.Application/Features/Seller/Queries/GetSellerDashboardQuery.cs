using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Seller;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Seller.Queries
{
    public class GetSellerDashboardQuery : IRequest<BaseResponse<SellerDashboardResponse>>
    {
        public Guid ShopId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Period { get; set; } = "month"; // day, month, year
    }
}
