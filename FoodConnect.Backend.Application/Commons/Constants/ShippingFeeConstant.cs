namespace FoodConnect.Backend.Application.Commons.Constants
{
    public static class ShippingFeeConstant
    {
        public const decimal EXPRESS_BASE_FEE = 5000;
        
        public const decimal EXPRESS_PRICE_PER_KM = 5000;
        
        public const decimal EXPRESS_FREE_DISTANCE = 2.0m;
        
        public const decimal EXPRESS_MAX_DISTANCE = 20.0m;

        public const decimal STANDARD_SAME_PROVINCE_FEE = 20000;
        
        public const decimal STANDARD_DIFFERENT_PROVINCE_FEE = 30000;
    }
}
