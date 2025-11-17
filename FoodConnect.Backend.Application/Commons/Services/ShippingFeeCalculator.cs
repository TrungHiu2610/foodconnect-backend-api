using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Services
{
    public class ShippingFeeCalculatorService : IShippingFeeCalculatorService
    {
        public decimal CalculateShippingFee(
            DeliveryTypeEnum deliveryType, 
            double distanceInKm,
            string? buyerProvince = null,
            string? shopProvince = null)
        {
            if (deliveryType == DeliveryTypeEnum.Express)
            {
                return CalculateExpressShippingFee((decimal)distanceInKm);
            }
            else // Standard
            {
                return CalculateStandardShippingFee(buyerProvince, shopProvince);
            }
        }

        public bool ValidateExpressDeliveryDistance(double distanceInKm)
        {
            return (decimal)distanceInKm <= ShippingFeeConstant.EXPRESS_MAX_DISTANCE;
        }

        /// Express: Base fee + (Distance - 2km) * Price per km
        /// Example: 5km = 5000 + (5-2)*5000 = 5000 + 15000 = 20,000 VND
        private decimal CalculateExpressShippingFee(decimal distanceInKm)
        {
            if (distanceInKm <= ShippingFeeConstant.EXPRESS_FREE_DISTANCE)
            {
                return ShippingFeeConstant.EXPRESS_BASE_FEE;
            }

            decimal extraDistance = distanceInKm - ShippingFeeConstant.EXPRESS_FREE_DISTANCE;
            return RoundUpToThousand(ShippingFeeConstant.EXPRESS_BASE_FEE + (extraDistance * ShippingFeeConstant.EXPRESS_PRICE_PER_KM));
        }

        private decimal CalculateStandardShippingFee(string? buyerProvince, string? shopProvince)
        {
            if (string.IsNullOrEmpty(buyerProvince) || string.IsNullOrEmpty(shopProvince))
            {
                return RoundUpToThousand(ShippingFeeConstant.STANDARD_DIFFERENT_PROVINCE_FEE); // Default to higher fee if province info missing
            }

            bool isSameProvince = buyerProvince.Trim().Equals(shopProvince.Trim(), StringComparison.OrdinalIgnoreCase);
            return RoundUpToThousand(isSameProvince ? ShippingFeeConstant.STANDARD_SAME_PROVINCE_FEE : ShippingFeeConstant.STANDARD_DIFFERENT_PROVINCE_FEE);
        }
        private decimal RoundUpToThousand(decimal amount)
        {
            return Math.Ceiling(amount / 1000) * 1000;
        }
    }
}
