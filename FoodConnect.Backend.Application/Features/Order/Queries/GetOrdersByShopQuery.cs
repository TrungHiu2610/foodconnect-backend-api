using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByShopQuery : IRequest<BaseResponse<PaginatedList<OrderSummaryDto>>>
    {
        public Guid ShopId { get; set; }
        public OrderStatusEnum? Status { get; set; }
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
