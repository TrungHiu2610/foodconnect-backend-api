using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetListCategoryQueryHandler : IRequestHandler<GetListCategoryQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetListCategoryQueryHandler(ICategoryRepository categoryRepository) 
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<BaseResponse<GetListCategoryResponse>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();
            var listCategories = await _categoryRepository.GetListCategoryResponseAsync();
            if (listCategories == null || listCategories.Categories == null)
            {
                return result.BuildFail("No categories found");
            }
            return result.BuildSuccess(listCategories, "Get list categories successfully");
        }
    }
}
