using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.Services
{
    public class ShippingFeeCalculator
    {
        public double CalculateShippingFee(
            DeliveryTypeEnum deliveryType, 
            double distanceInKm,
            string? buyerProvince,
            string? shopProvince)
        {
            if (deliveryType == DeliveryTypeEnum.Express)
            {
                return CalculateExpressShippingFee(distanceInKm);
            }
            else // Standard
            {
                return CalculateStandardShippingFee(buyerProvince, shopProvince);
            }
        }

        /// Express: Base fee + (Distance - 2km) * Price per km
        /// Example: 5km = 5000 + (5-2)*5000 = 5000 + 15000 = 20,000 VND
        private double CalculateExpressShippingFee(double distanceInKm)
        {
            if (distanceInKm <= ShippingFeeConstant.EXPRESS_FREE_DISTANCE)
            {
                return ShippingFeeConstant.EXPRESS_BASE_FEE;
            }

            double extraDistance = distanceInKm - ShippingFeeConstant.EXPRESS_FREE_DISTANCE;
            return ShippingFeeConstant.EXPRESS_BASE_FEE + (extraDistance * ShippingFeeConstant.EXPRESS_PRICE_PER_KM);
        }

        private double CalculateStandardShippingFee(string? buyerProvince, string? shopProvince)
        {
            if (string.IsNullOrEmpty(buyerProvince) || string.IsNullOrEmpty(shopProvince))
            {
                return ShippingFeeConstant.STANDARD_DIFFERENT_PROVINCE_FEE; // Default to higher fee if province info missing
            }

            bool isSameProvince = buyerProvince.Trim().Equals(shopProvince.Trim(), StringComparison.OrdinalIgnoreCase);
            return isSameProvince ? ShippingFeeConstant.STANDARD_SAME_PROVINCE_FEE : ShippingFeeConstant.STANDARD_DIFFERENT_PROVINCE_FEE;
        }

        public bool ValidateExpressDeliveryDistance(double distanceInKm)
        {
            return distanceInKm <= ShippingFeeConstant.EXPRESS_MAX_DISTANCE;
        }

        public double GetMaxExpressDistance()
        {
            return ShippingFeeConstant.EXPRESS_MAX_DISTANCE;
        }
    }
}
