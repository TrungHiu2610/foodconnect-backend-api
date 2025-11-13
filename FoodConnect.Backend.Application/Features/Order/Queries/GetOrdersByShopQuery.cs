using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByShopQuery : IRequest<BaseResponse<List<OrderSummaryDto>>>
    {
        public Guid ShopId { get; set; }
        public OrderStatusEnum? Status { get; set; }
    }
}
