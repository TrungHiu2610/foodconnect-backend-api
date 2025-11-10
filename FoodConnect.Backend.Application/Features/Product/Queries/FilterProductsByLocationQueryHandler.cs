using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Services;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class FilterProductsByLocationQueryHandler : IRequestHandler<FilterProductsByLocationQuery, BaseResponse<List<GetListProductItemResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly DistanceCalculator _distanceCalculator;
        private readonly IMapper _mapper;

        public FilterProductsByLocationQueryHandler(
            IProductRepository productRepository,
            IShopRepository shopRepository,
            ICategoryRepository categoryRepository,
            DistanceCalculator distanceCalculator,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _categoryRepository = categoryRepository;
            _distanceCalculator = distanceCalculator;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<GetListProductItemResponse>>> Handle(FilterProductsByLocationQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<GetListProductItemResponse>>();

            // Step 1: Get all active products with shop and category info
            var products = await _productRepository.GetAllProductsWithDetailsAsync();
            if (products == null || !products.Any())
            {
                return result.BuildSuccess(new List<GetListProductItemResponse>(), "No products available");
            }

            // Step 2: Check if buyer has valid coordinates
            bool buyerHasLocation = request.BuyerLatitude.HasValue && request.BuyerLongitude.HasValue;

            var filteredProducts = new List<Domain.Entities.Product>();

            foreach (var product in products)
            {
                // Get product's category delivery type
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                if (category == null) continue;

                var deliveryType = category.DeliveryType;

                // Step 3: If buyer has NO location
                if (!buyerHasLocation)
                {
                    // Only show Standard products
                    if (deliveryType == DeliveryTypeEnum.Standard)
                    {
                        filteredProducts.Add(product);
                    }
                    continue;
                }

                // Step 4: Buyer HAS location → calculate distance to shop
                var shop = product.Shop;
                if (shop == null) continue;

                // Express products: only show if shop within delivery range
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

                        // Only include if within express delivery range
                        if (distance <= ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                        {
                            filteredProducts.Add(product);
                        }
                    }
                    // If shop has no coordinates, don't show express products
                }
                else // Standard products: always show (no distance limit)
                {
                    filteredProducts.Add(product);
                }
            }

            // Step 5: Map to response DTOs
            var productResponses = _mapper.Map<List<GetListProductItemResponse>>(filteredProducts);

            return result.BuildSuccess(productResponses, $"Found {productResponses.Count} products");
        }
    }
}
