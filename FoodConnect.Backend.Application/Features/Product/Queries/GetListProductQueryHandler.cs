using AutoMapper;
using FoodConnect.Backend.Application.Commons.Constants;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Extensions;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDistanceCalculatorService _distanceCalculator;
        private readonly IMapper _mapper;
        
        private static readonly Dictionary<string, Expression<Func<Domain.Entities.Product, object>>> _sortableColumns =
            new Dictionary<string, Expression<Func<Domain.Entities.Product, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "name", p => p.Name },
                { "price", p => p.Price },
                { "createdAt", p => p.CreatedAtUtc }
            };

        public GetListProductQueryHandler(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IAddressRepository addressRepository,
            ICurrentUserService currentUserService,
            IDistanceCalculatorService distanceCalculator,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _addressRepository = addressRepository;
            _currentUserService = currentUserService;
            _distanceCalculator = distanceCalculator;
            _mapper = mapper;
        }
        public async Task<BaseResponse<PaginatedList<GetListProductItemResponse>>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<GetListProductItemResponse>>();

            // Get buyer location (from params or default address)
            var buyerLocation = await GetBuyerLocationAsync(request);

            var query = _productRepository.GetProductsAsQueryable()
                .Include(p => p.ProductAssets)
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .AsNoTracking();
            
            // Apply filters
            query = await ApplyFiltersAsync(query, request, cancellationToken);

            // Apply sorting
            query = ApplySorting(query, request);

            // Apply text search and location filtering
            var (filteredProducts, totalCount) = await ApplySearchAndLocationFilterAsync(
                query, request, buyerLocation, cancellationToken);

            // Paginate
            var paginatedProducts = filteredProducts
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var productDtos = _mapper.Map<List<GetListProductItemResponse>>(paginatedProducts);
            var paginatedList = new PaginatedList<GetListProductItemResponse>(
                productDtos, totalCount, request.PageNumber, request.PageSize);

            return result.BuildSuccess(paginatedList, "Get list products successfully");
        }

        #region Private Helper Methods
        private async Task<(double? Latitude, double? Longitude)> GetBuyerLocationAsync(GetListProductQuery request)
        {
            // If location provided in request, use it
            if (request.BuyerLatitude.HasValue && request.BuyerLongitude.HasValue)
            {
                return (request.BuyerLatitude, request.BuyerLongitude);
            }

            // Try to get from user's default address
            var userId = _currentUserService.UserId ?? request.UserId;
            if (userId.HasValue)
            {
                var defaultAddress = await _addressRepository.GetDefaultAddressByUserIdAsync(userId.Value);
                if (defaultAddress?.Latitude != null && defaultAddress?.Longitude != null)
                {
                    return (defaultAddress.Latitude, defaultAddress.Longitude);
                }
            }

            return (null, null);
        }
        private async Task<IQueryable<Domain.Entities.Product>> ApplyFiltersAsync(
            IQueryable<Domain.Entities.Product> query, 
            GetListProductQuery request,
            CancellationToken cancellationToken)
        {
            // Category filter (including parent category children)
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, c => c.Parent);
                if (category != null && category.Parent == null)
                {
                    // Parent category: include all children
                    var childrenCategory = await _categoryRepository.GetChildrenByParentIdAsync(category.Id);
                    var childCategoryIds = childrenCategory.Select(c => c.Id).ToList();
                    query = query.Where(p => childCategoryIds.Contains(p.CategoryId));
                }
                else
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId);
                }
            }

            // Shop filter
            if (request.ShopId.HasValue)
            {
                query = query.Where(p => p.ShopId == request.ShopId);
            }

            // Availability filter
            if (request.IsAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (Enum.TryParse<ProductStatusEnum>(request.Status, true, out var statusEnum))
                {
                    query = query.Where(p => p.Status == statusEnum);
                }
            }

            return query;
        }
        private IQueryable<Domain.Entities.Product> ApplySorting(
            IQueryable<Domain.Entities.Product> query, 
            GetListProductQuery request)
        {
            if (request.SortInfos != null && request.SortInfos.Any())
            {
                IOrderedQueryable<Domain.Entities.Product>? orderedQuery = null;
                
                // Shop management: Sort out-of-stock products last
                if (request.SortOutOfStockLast)
                {
                    orderedQuery = query.OrderByDescending(p => p.IsAvailable);
                }

                foreach (var sortInfo in request.SortInfos)
                {
                    if (!_sortableColumns.TryGetValue(sortInfo.PropertyName, out var keySelector))
                    {
                        continue; 
                    }

                    if (orderedQuery == null) 
                    {
                        orderedQuery = sortInfo.IsAscending
                            ? query.OrderBy(keySelector)
                            : query.OrderByDescending(keySelector);
                    }
                    else 
                    {
                        orderedQuery = sortInfo.IsAscending
                            ? orderedQuery.ThenBy(keySelector)
                            : orderedQuery.ThenByDescending(keySelector);
                    }
                }
                query = orderedQuery ?? query.OrderByDescending(p => p.CreatedAtUtc);
            }
            else
            {
                // Default sort
                if (request.SortOutOfStockLast)
                {
                    query = query.OrderByDescending(p => p.IsAvailable)
                                 .ThenByDescending(p => p.CreatedAtUtc);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAtUtc);
                }
            }

            return query;
        }

        private async Task<(List<Domain.Entities.Product> Products, int TotalCount)> ApplySearchAndLocationFilterAsync(
            IQueryable<Domain.Entities.Product> query,
            GetListProductQuery request,
            (double? Latitude, double? Longitude) buyerLocation,
            CancellationToken cancellationToken)
        {
            var allProducts = await query.ToListAsync(cancellationToken);

            // Apply text search
            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                var normalizedSearch = request.TextSearch.NormalizeForSearch();
                allProducts = allProducts.Where(p =>
                    (p.Name != null && p.Name.NormalizeForSearch().Contains(normalizedSearch)) ||
                    (p.Description != null && p.Description.NormalizeForSearch().Contains(normalizedSearch))
                ).ToList();
            }

            // Apply location-based filtering for delivery type
            var filteredProducts = ApplyLocationFilter(allProducts, request, buyerLocation);

            return (filteredProducts, filteredProducts.Count);
        }

        private List<Domain.Entities.Product> ApplyLocationFilter(
            List<Domain.Entities.Product> products,
            GetListProductQuery request,
            (double? Latitude, double? Longitude) buyerLocation)
        {
            bool hasLocation = buyerLocation.Latitude.HasValue && buyerLocation.Longitude.HasValue;
            var filteredProducts = new List<Domain.Entities.Product>();

            foreach (var product in products)
            {
                if (product.Category == null) continue;

                var deliveryType = product.Category.DeliveryType;

                // Filter by delivery type if specified
                if (request.DeliveryType.HasValue && deliveryType != request.DeliveryType.Value)
                {
                    continue;
                }

                // If buyer has no location
                if (!hasLocation)
                {
                    // Only show Standard products
                    if (deliveryType == DeliveryTypeEnum.Standard)
                    {
                        filteredProducts.Add(product);
                    }
                    continue;
                }

                // Buyer has location
                if (deliveryType == DeliveryTypeEnum.Express)
                {
                    // Express: Only show if shop within range
                    if (product.Shop?.Latitude != null && product.Shop?.Longitude != null)
                    {
                        double distance = _distanceCalculator.CalculateDistance(
                            buyerLocation.Latitude!.Value,
                            buyerLocation.Longitude!.Value,
                            product.Shop.Latitude.Value,
                            product.Shop.Longitude.Value
                        );

                        if (distance <= (double)ShippingFeeConstant.EXPRESS_MAX_DISTANCE)
                        {
                            product.CalculatedDistance = distance; // Set for later use
                            filteredProducts.Add(product);
                        }
                    }
                }
                else // Standard
                {
                    if (product.Shop?.Latitude != null && product.Shop?.Longitude != null)
                    {
                        double distance = _distanceCalculator.CalculateDistance(
                            buyerLocation.Latitude!.Value,
                            buyerLocation.Longitude!.Value,
                            product.Shop.Latitude.Value,
                            product.Shop.Longitude.Value
                        );
                        product.CalculatedDistance = distance;
                    }
                    filteredProducts.Add(product);
                }
            }

            return filteredProducts;
        }

        #endregion
    }
}
