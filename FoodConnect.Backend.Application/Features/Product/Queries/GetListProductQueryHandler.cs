using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using Microsoft.EntityFrameworkCore;
using FoodConnect.Backend.Application.Commons.Models;
using System.Linq.Expressions;
using FoodConnect.Backend.Application.Commons.Extensions;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private static readonly Dictionary<string, Expression<Func<Domain.Entities.Product, object>>> _sortableColumns =
            new Dictionary<string, Expression<Func<Domain.Entities.Product, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                { "name", p => p.Name },
                { "price", p => p.Price },
                { "createdAt", p => p.CreatedAtUtc }
            };

        public GetListProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }
        public async Task<BaseResponse<PaginatedList<GetListProductItemResponse>>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<GetListProductResponse>>();

            var query = _productRepository.GetProductsAsQueryable().Include(p => p.ProductAssets).Include(p=>p.Shop).AsNoTracking();
            
            // 1. filter
            if (request.CategoryId != null)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId);
            }

            if (request.ShopId != null)
            {
                query = query.Where(p => p.ShopId == request.ShopId);
            }

            if (request.IsAvailable.HasValue)
            {
                query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (Enum.TryParse<Domain.Enums.ProductStatusEnum>(request.Status, true, out var statusEnum))
                {
                    query = query.Where(p => p.Status == statusEnum);
                }
            }

            // 2. search - Use case-insensitive search at DB level first
            // Then apply Vietnamese normalization client-side for better accuracy
            var hasTextSearch = !string.IsNullOrEmpty(request.TextSearch);
            if (hasTextSearch)
            {
                // DB-level filter: Case-insensitive contains (reduces data significantly)
                var lowerSearch = request.TextSearch!.ToLower();
                query = query.Where(p => 
                    (p.Name != null && p.Name.ToLower().Contains(lowerSearch)) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerSearch))
                );
            }

            // 3. sort
            if (request.SortInfos != null && request.SortInfos.Any())
            {
                IOrderedQueryable<Domain.Entities.Product>? orderedQuery = null;
                
                // If shop management view: Sort out-of-stock products last
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
                    // Shop management: Available products first, then sort by date
                    query = query.OrderByDescending(p => p.IsAvailable)
                                 .ThenByDescending(p => p.CreatedAtUtc);
                }
                else
                {
                    // Buyer view: Just sort by date
                    query = query.OrderByDescending(p => p.CreatedAtUtc);
                }
            }

            // 4. Execute query and apply Vietnamese normalization if needed
            if (hasTextSearch)
            {
                // Fetch filtered products from DB (already reduced by case-insensitive filter)
                var allFilteredProducts = await query.ToListAsync(cancellationToken);
                
                // Apply Vietnamese diacritics removal for accurate matching
                // Example: "hu tieu" matches "Hủ tiếu"
                var normalizedSearch = request.TextSearch!.NormalizeForSearch();
                var matchedProducts = allFilteredProducts.Where(p =>
                    (p.Name != null && p.Name.NormalizeForSearch().Contains(normalizedSearch)) ||
                    (p.Description != null && p.Description.NormalizeForSearch().Contains(normalizedSearch))
                ).ToList();
                
                // Paginate
                var totalItems = matchedProducts.Count;
                var products = matchedProducts
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var productDtos = _mapper.Map<List<GetListProductItemResponse>>(products);
                var paginatedList = new PaginatedList<GetListProductItemResponse>(productDtos, totalItems, request.PageNumber, request.PageSize);

                return new BaseResponse<PaginatedList<GetListProductItemResponse>>().BuildSuccess(paginatedList, "Get list products successfully");
            }
            else
            {
                // No text search - use efficient DB pagination
                var totalItems = await query.CountAsync(cancellationToken);
                var products = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var productDtos = _mapper.Map<List<GetListProductItemResponse>>(products);
                var paginatedList = new PaginatedList<GetListProductItemResponse>(productDtos, totalItems, request.PageNumber, request.PageSize);

                return new BaseResponse<PaginatedList<GetListProductItemResponse>>().BuildSuccess(paginatedList, "Get list products successfully");
            }
        }
    }
}
