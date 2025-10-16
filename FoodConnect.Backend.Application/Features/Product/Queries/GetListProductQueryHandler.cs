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

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, BaseResponse<PaginatedList<GetListProductResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetListProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }
        public async Task<BaseResponse<PaginatedList<GetListProductResponse>>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<GetListProductResponse>>();

            var query = _productRepository.GetProductsAsQueryable().Include(p => p.ProductAssets).AsNoTracking();
            // 1. filter
            if (request.CategoryId != null)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId);
            }

            // 2. search
            if (!string.IsNullOrEmpty(request.TextSearch))
            {
                query = query.Where(p => p.Name.Contains(request.TextSearch) || p.Description.Contains(request.TextSearch));
            }

            // 3. sort
            if(request.SortInfos!=null && request.SortInfos.Any())
            {
                IOrderedQueryable<Domain.Entities.Product>? orderedQuery = null;
                for (int i = 0; i < request.SortInfos.Count; i++)
                {
                    var sortInfo = request.SortInfos[i];
                    if (i == 0)
                    {
                        orderedQuery = sortInfo.IsAscending
                            ? query.OrderBy(e => EF.Property<object>(e, sortInfo.PropertyName))
                            : query.OrderByDescending(e => EF.Property<object>(e, sortInfo.PropertyName));
                    }
                    else
                    {
                        orderedQuery = sortInfo.IsAscending
                            ? orderedQuery.ThenBy(e => EF.Property<object>(e, sortInfo.PropertyName))
                            : orderedQuery.ThenByDescending(e => EF.Property<object>(e, sortInfo.PropertyName));
                    }
                }
                query = orderedQuery ?? query;
            }
            else
            {
                // Default sort by CreatedAt descending
                query = query.OrderByDescending(p => p.CreatedAtUtc);
                //query = query.OrderByDescending(p => p.Rating);
            }

            // 4. paginate
            var totalItems = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var productResponses = _mapper.Map<List<GetListProductItemResponse>>(products);
            var listProductResponses = new GetListProductResponse
            {
                Products = productResponses
            };
            var paginatedList = new PaginatedList<GetListProductResponse>(listProductResponses, totalItems, request.PageNumber, request.PageSize);

            return result.BuildSuccess(paginatedList, "Get list products successfully");
        }
    }
}
