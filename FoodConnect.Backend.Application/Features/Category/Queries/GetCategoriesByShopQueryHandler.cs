using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Category;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Category.Queries
{
    public class GetCategoriesByShopQueryHandler 
        : IRequestHandler<GetCategoriesByShopQuery, BaseResponse<GetListCategoryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetCategoriesByShopQueryHandler(
            ICategoryRepository categoryRepository,
            IShopRepository shopRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _shopRepository = shopRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<GetListCategoryResponse>> Handle(
            GetCategoriesByShopQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetListCategoryResponse>();

            // Authorization: Verify user is the owner of the shop
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized();
            }

            var shop = await _shopRepository.GetByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            if (shop.UserId != userId.Value)
            {
                return result.BuildForbidden("You don't have permission to access this shop's categories");
            }

            // Get all category IDs for this shop (including children)
            var categoryIds = await _shopRepository.GetAllCategoryIdsForShopAsync(request.ShopId);

            if (!categoryIds.Any())
            {
                return result.BuildSuccess(
                    new GetListCategoryResponse { Categories = new List<GetListCategoryItem>() },
                    "No categories found for this shop"
                );
            }

            // Get categories by IDs
            var categories = await _categoryRepository.GetCategoriesByIdsAsync(categoryIds);

            var listItemResponse = _mapper.Map<List<GetListCategoryItem>>(categories);
            var response = new GetListCategoryResponse
            {
                Categories = listItemResponse
            };

            return result.BuildSuccess(response, "Get shop categories successfully");
        }
    }
}
