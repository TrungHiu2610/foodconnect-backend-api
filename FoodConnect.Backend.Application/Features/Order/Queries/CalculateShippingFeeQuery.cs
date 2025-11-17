using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class CalculateShippingFeeQuery : IRequest<BaseResponse<CalculateShippingFeeResponse>>
    {
        public Guid ShopId { get; set; }
        public DeliveryTypeEnum DeliveryType { get; set; }
        public double BuyerLatitude { get; set; }
        public double BuyerLongitude { get; set; }
        public string? BuyerProvince { get; set; }
    }

    public class CalculateShippingFeeResponse
    {
        public decimal ShippingFee { get; set; }
        public double DistanceKm { get; set; }
        public DeliveryTypeEnum DeliveryType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
