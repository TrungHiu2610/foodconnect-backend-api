using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetChildrenCategoriesByParentQueryHandler 
        : IRequestHandler<GetChildrenCategoriesByParentQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetChildrenCategoriesByParentQueryHandler(
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<GetListCategoryResponse>> Handle(
            GetChildrenCategoriesByParentQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();

            // Validate parent exists
            var parent = await _categoryRepository.GetByIdAsync(request.ParentId);
            if (parent == null)
            {
                return result.BuildNotFound("Parent category not found");
            }

            // Get children
            var children = await _categoryRepository.GetChildrenByParentIdAsync(request.ParentId);

            if (!children.Any())
            {
                return result.BuildSuccess(
                    new GetListCategoryResponse { Categories = new List<GetListCategoryItem>() },
                    "No children categories found"
                );
            }

            var listItemResponse = _mapper.Map<List<GetListCategoryItem>>(children);
            var response = new GetListCategoryResponse
            {
                Categories = listItemResponse
            };

            return result.BuildSuccess(response, "Get children categories successfully");
        }
    }
}
