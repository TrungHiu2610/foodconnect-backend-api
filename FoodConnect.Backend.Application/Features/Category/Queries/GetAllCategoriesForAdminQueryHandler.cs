using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using FoodConnect.Backend.Application.Commons.Extensions;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetAllCategoriesForAdminQueryHandler : IRequestHandler<GetAllCategoriesForAdminQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetAllCategoriesForAdminQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<GetListCategoryResponse>> Handle(GetAllCategoriesForAdminQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();

            var allCategories = await _categoryRepository.GetAllCategoriesWithDetailsAsync();
            if (allCategories == null || !allCategories.Any())
            {
                return result.BuildFail("No categories found");
            }

            var filteredCategories = allCategories.ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var normalizedSearch = request.SearchTerm.NormalizeForSearch();
                filteredCategories = filteredCategories.Where(c =>
                    // Vietnamese diacritics insensitive search
                    (c.Name != null && c.Name.NormalizeForSearch().Contains(normalizedSearch)) ||
                    (c.Description != null && c.Description.NormalizeForSearch().Contains(normalizedSearch)) ||
                    (c.Parent != null && c.Parent.Name != null && c.Parent.Name.NormalizeForSearch().Contains(normalizedSearch))
                ).ToList();
            }

            if (request.IsActive.HasValue)
            {
                filteredCategories = filteredCategories.Where(c => c.IsActive == request.IsActive.Value).ToList();
            }

            if (request.ParentId.HasValue)
            {
                filteredCategories = filteredCategories.Where(c => c.ParentId == request.ParentId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.DeliveryType))
            {
                if (Enum.TryParse<DeliveryTypeEnum>(request.DeliveryType, true, out var deliveryType))
                {
                    filteredCategories = filteredCategories.Where(c => c.DeliveryType == deliveryType).ToList();
                }
            }

            if (request.CreatedFrom.HasValue)
            {
                filteredCategories = filteredCategories.Where(c => c.CreatedAtUtc >= request.CreatedFrom.Value).ToList();
            }

            if (request.CreatedTo.HasValue)
            {
                filteredCategories = filteredCategories.Where(c => c.CreatedAtUtc <= request.CreatedTo.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                filteredCategories = request.SortBy.ToLower() switch
                {
                    "name" => request.IsDescending
                        ? filteredCategories.OrderByDescending(c => c.Name).ToList()
                        : filteredCategories.OrderBy(c => c.Name).ToList(),
                    "createdat" => request.IsDescending
                        ? filteredCategories.OrderByDescending(c => c.CreatedAtUtc).ToList()
                        : filteredCategories.OrderBy(c => c.CreatedAtUtc).ToList(),
                    "productcount" => request.IsDescending
                        ? filteredCategories.OrderByDescending(c => c.Products != null ? c.Products.Count() : 0).ToList()
                        : filteredCategories.OrderBy(c => c.Products != null ? c.Products.Count() : 0).ToList(),
                    _ => filteredCategories.OrderByDescending(c => c.CreatedAtUtc).ToList()
                };
            }
            else
            {
                filteredCategories = filteredCategories.OrderByDescending(c => c.CreatedAtUtc).ToList();
            }

            var listItemResponse = _mapper.Map<List<GetListCategoryItem>>(filteredCategories);
            var response = new GetListCategoryResponse
            {
                Categories = listItemResponse
            };

            return result.BuildSuccess(response, "Get all categories for admin successfully");
        }
    }
}
