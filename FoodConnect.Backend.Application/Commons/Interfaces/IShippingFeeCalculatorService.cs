using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IShippingFeeCalculatorService
    {
        double CalculateShippingFee(double distanceKm, DeliveryTypeEnum deliveryType);
        bool IsExpressDeliveryAllowed(double distanceKm);
        double GetMaxExpressDeliveryDistance();
    }
}
