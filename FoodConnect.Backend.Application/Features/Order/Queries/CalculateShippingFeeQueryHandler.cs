using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Order.Queries
{
    public class CalculateShippingFeeQueryHandler : IRequestHandler<CalculateShippingFeeQuery, BaseResponse<CalculateShippingFeeResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IShippingFeeCalculatorService _shippingFeeCalculator;

        public CalculateShippingFeeQueryHandler(
            IShopRepository shopRepository,
            IDistanceCalculatorService distanceCalculator,
            IShippingFeeCalculatorService shippingFeeCalculator)
        {
            _shopRepository = shopRepository;
            _distanceCalculator = distanceCalculator;
            _shippingFeeCalculator = shippingFeeCalculator;
        }

        public async Task<BaseResponse<CalculateShippingFeeResponse>> Handle(CalculateShippingFeeQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CalculateShippingFeeResponse>();

            // Get shop
            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            // Validate shop has coordinates
            if (!shop.Latitude.HasValue || !shop.Longitude.HasValue)
            {
                return result.BuildFail("Shop does not have location coordinates configured");
            }

            // Calculate distance
            double distanceKm = _distanceCalculator.CalculateDistance(
                request.BuyerLatitude,
                request.BuyerLongitude,
                shop.Latitude.Value,
                shop.Longitude.Value
            );

            // Validate express delivery distance
            if (request.DeliveryType == DeliveryTypeEnum.Express)
            {
                if (!_shippingFeeCalculator.ValidateExpressDeliveryDistance(distanceKm))
                {
                    return result.BuildFail($"Express delivery is only available for distances up to {ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km. Distance to this shop is {distanceKm:F2}km. Please choose Standard delivery.");
                }
            }

            // Calculate shipping fee
            decimal shippingFee = _shippingFeeCalculator.CalculateShippingFee(
                request.DeliveryType,
                distanceKm,
                request.BuyerProvince,
                shop.City // Using City as Province
            );

            var response = new CalculateShippingFeeResponse
            {
                ShippingFee = shippingFee,
                DistanceKm = Math.Round(distanceKm, 2),
                DeliveryType = request.DeliveryType,
                Message = request.DeliveryType == DeliveryTypeEnum.Express
                    ? $"Express delivery (≤{ShippingFeeConstant.EXPRESS_MAX_DISTANCE}km): First 2km = 5,000 VND, then 5,000 VND/km"
                    : $"Standard delivery: Same province = 20,000 VND, Different province = 30,000 VND"
            };

            return result.BuildSuccess(response, "Shipping fee calculated successfully");
        }
    }
}
