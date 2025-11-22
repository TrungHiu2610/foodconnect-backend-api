using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Features.Order.DTOs;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class GetOrdersByBuyerQuery : IRequest<BaseResponse<List<OrderSummaryDto>>>
    {
        public OrderStatusEnum? Status { get; set; }
        public OrderReviewStatusEnum? ReviewStatus { get; set; }
    }
}
