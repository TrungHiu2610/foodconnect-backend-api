using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class FilterShopsByLocationQueryHandler : IRequestHandler<FilterShopsByLocationQuery, BaseResponse<List<ShopResponse>>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IMapper _mapper;

        public FilterShopsByLocationQueryHandler(
            IShopRepository shopRepository,
            ICategoryRepository categoryRepository,
            IDistanceCalculatorService distanceCalculator,
            IMapper mapper)
        {
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _distanceCalculator = distanceCalculator;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<ShopResponse>>> Handle(FilterShopsByLocationQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ShopResponse>>();

            var shops = await _shopRepository.GetAllActiveShopsAsync();
            if (shops == null || !shops.Any())
            {
                return result.BuildSuccess(new List<ShopResponse>(), "No shops available");
            }

            var filteredShops = new List<(Domain.Entities.Shop shop, double? distance)>();

            bool buyerHasLocation = request.BuyerLatitude.HasValue && request.BuyerLongitude.HasValue;

            foreach (var shop in shops)
            {
                var shopCategoryIds = shop.ShopCategories.Select(sc => sc.CategoryId).ToList();
                var categories = await _categoryRepository.GetCategoriesByIdsAsync(shopCategoryIds);
                var deliveryTypes = categories.Select(c => c.DeliveryType).Distinct().ToList();

                bool hasExpress = deliveryTypes.Contains(DeliveryTypeEnum.Express);
                bool hasStandard = deliveryTypes.Contains(DeliveryTypeEnum.Standard);

                if (!buyerHasLocation)
                {
                    if (hasStandard)
                    {
                        if (!request.DeliveryType.HasValue || request.DeliveryType == DeliveryTypeEnum.Standard)
                        {
                            filteredShops.Add((shop, null));
                        }
                    }
                    continue;
                }

                double? distance = null;
                if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                {
                    distance = _distanceCalculator.CalculateDistance(
                        request.BuyerLatitude.Value,
                        request.BuyerLongitude.Value,
                        shop.Latitude.Value,
                        shop.Longitude.Value
                    );
                }

                bool shouldInclude = false;

                if (request.DeliveryType.HasValue)
                {
                    if (request.DeliveryType == DeliveryTypeEnum.Express)
                    {
                        if (hasExpress && distance.HasValue && distance.Value <= (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                        {
                            shouldInclude = true;
                        }
                    }
                    else // Standard
                    {
                        if (hasStandard)
                        {
                            shouldInclude = true;
                        }
                    }
                }
                else // No filter → show both
                {
                    if (hasExpress && distance.HasValue && distance.Value <= (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                    {
                        shouldInclude = true;
                    }
                    else if (hasStandard)
                    {
                        shouldInclude = true;
                    }
                }

                if (shouldInclude)
                {
                    filteredShops.Add((shop, distance));
                }
            }

            var sortedShops = filteredShops
                .OrderBy(x => x.distance ?? double.MaxValue)
                .Select(x => x.shop)
                .ToList();

            var shopResponses = new List<ShopResponse>();
            foreach (var shop in sortedShops)
            {
                var shopResponse = _mapper.Map<ShopResponse>(shop);
                
                var distanceInfo = filteredShops.FirstOrDefault(f => f.shop.Id == shop.Id);
                shopResponse.CalculatedDistance = distanceInfo.distance;

                shopResponses.Add(shopResponse);
            }

            return result.BuildSuccess(shopResponses, $"Found {shopResponses.Count} shops");
        }
    }
}
