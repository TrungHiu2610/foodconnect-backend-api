using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetListCategoryQueryHandler : IRequestHandler<GetListCategoryQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetListCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper) 
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        public async Task<BaseResponse<GetListCategoryResponse>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();

            var listCategories = await _categoryRepository.GetActiveCategoriesAsync();
            if (listCategories == null || !listCategories.Any())
            {
                return result.BuildFail("No categories found");
            }
            var listItemResponse = _mapper.Map<List<GetListCategoryItem>>(listCategories);
            var response = new GetListCategoryResponse
            {
                Categories = listItemResponse
            };

            return result.BuildSuccess(response, "Get list categories successfully");
        }
    }
}
