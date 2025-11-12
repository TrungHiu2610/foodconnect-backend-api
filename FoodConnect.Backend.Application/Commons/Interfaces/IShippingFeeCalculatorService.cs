using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IShippingFeeCalculatorService
    {
        decimal CalculateShippingFee(DeliveryTypeEnum deliveryType, double distanceInKm, string? buyerProvince = null, string? shopProvince = null);
        bool ValidateExpressDeliveryDistance(double distanceInKm);
    }
}
