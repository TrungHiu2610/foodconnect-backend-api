using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetParentCategoriesQueryHandler : IRequestHandler<GetParentCategoriesQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetParentCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<GetListCategoryResponse>> Handle(GetParentCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();

            var parentCategories = await _categoryRepository.GetParentCategoriesAsync();
            if (parentCategories == null || !parentCategories.Any())
            {
                return result.BuildFail("No parent categories found");
            }

            var listItemResponse = _mapper.Map<List<GetListCategoryItem>>(parentCategories);
            var response = new GetListCategoryResponse
            {
                Categories = listItemResponse
            };

            return result.BuildSuccess(response, "Get parent categories successfully");
        }
    }
}
