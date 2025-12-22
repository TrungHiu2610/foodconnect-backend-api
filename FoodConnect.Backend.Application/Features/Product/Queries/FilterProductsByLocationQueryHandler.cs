using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class FilterProductsByLocationQueryHandler : IRequestHandler<FilterProductsByLocationQuery, BaseResponse<List<GetListProductItemResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IMapper _mapper;

        public FilterProductsByLocationQueryHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IDistanceCalculatorService distanceCalculator,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _distanceCalculator = distanceCalculator;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<GetListProductItemResponse>>> Handle(FilterProductsByLocationQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<GetListProductItemResponse>>();

            var products = await _productRepository.GetAllProductsWithDetailsAsync();
            if (products == null || !products.Any())
            {
                return result.BuildSuccess(new List<GetListProductItemResponse>(), "No products available");
            }

            bool buyerHasLocation = request.BuyerLatitude.HasValue && request.BuyerLongitude.HasValue;
            if (!buyerHasLocation)
            {
                var userId = _currentUserService.UserId;
                var userDefaultAddress = null as Domain.Entities.Address;
                if (userId != null)
                {
                    userDefaultAddress = await _userRepository.GetDefaultAddressByIdAsync(userId.Value);
                    if (userDefaultAddress != null || userDefaultAddress.Latitude.HasValue || userDefaultAddress.Longitude.HasValue)
                    {
                        request.BuyerLatitude = userDefaultAddress.Latitude;
                        request.BuyerLongitude = userDefaultAddress.Longitude;
                        buyerHasLocation = true;
                    }
                }
            }

            var filteredProducts = new List<Domain.Entities.Product>();

            foreach (var product in products)
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                if (category == null) continue;

                var deliveryType = category.DeliveryType;

                if (!buyerHasLocation)
                {
                    if (deliveryType == DeliveryTypeEnum.Standard)
                    {
                        filteredProducts.Add(product);
                    }
                    continue;
                }

                var shop = product.Shop;
                if (shop == null) continue;

                if (deliveryType == DeliveryTypeEnum.Express)
                {
                    if (shop.Latitude.HasValue && shop.Longitude.HasValue)
                    {
                        double distance = _distanceCalculator.CalculateDistance(
                            request.BuyerLatitude.Value,
                            request.BuyerLongitude.Value,
                            shop.Latitude.Value,
                            shop.Longitude.Value
                        );

                        if (distance <= (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                        {
                            filteredProducts.Add(product);
                        }
                    }
                }
                else // Standard products: always show (no distance limit)
                {
                    filteredProducts.Add(product);
                }
            }

            var productResponses = _mapper.Map<List<GetListProductItemResponse>>(filteredProducts);

            return result.BuildSuccess(productResponses, $"Found {productResponses.Count} products");
        }
    }
}
