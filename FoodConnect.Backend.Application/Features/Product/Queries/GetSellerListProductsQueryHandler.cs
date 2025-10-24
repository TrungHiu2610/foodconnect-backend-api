using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Commons.Models;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetSellerListProductsQueryHandler : IRequestHandler<GetSellerListProductsQuery, BaseResponse<PaginatedList<GetListProductItemResponse>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public GetSellerListProductsQueryHandler(
            IProductRepository productRepository,
            ICurrentUserService currentUserService,
            IShopRepository shopRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
            _shopRepository = shopRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PaginatedList<GetListProductItemResponse>>> Handle(GetSellerListProductsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PaginatedList<GetListProductItemResponse>>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildFail("User not found");
            }

            // Lấy shop của seller
            var shop = await _shopRepository.GetByUserIdAsync((Guid)userId);
            if (shop == null)
            {
                return result.BuildFail("Shop not found. You must be approved as seller first.");
            }

            // Get products with pagination and filter
            var (products, totalCount) = await _productRepository.GetPagedByShopIdAsync(
                shop.Id,
                request.Page,
                request.PageSize,
                request.Status,
                request.SearchTerm);

            var items = _mapper.Map<List<GetListProductItemResponse>>(products);
            var response = new PaginatedList<GetListProductItemResponse>(items, totalCount, request.Page, request.PageSize);

            return result.BuildSuccess(response, "Get seller products successfully");
        }
    }
}
