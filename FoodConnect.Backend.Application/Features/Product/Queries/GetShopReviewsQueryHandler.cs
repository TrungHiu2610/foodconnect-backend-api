using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetShopReviewsQueryHandler : IRequestHandler<GetShopReviewsQuery, BaseResponse<PagedResponse<ProductReviewResponse>>>
    {
        private readonly IProductReviewRepository _reviewRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public GetShopReviewsQueryHandler(
            IProductReviewRepository reviewRepository,
            IShopRepository shopRepository,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _shopRepository = shopRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PagedResponse<ProductReviewResponse>>> Handle(GetShopReviewsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PagedResponse<ProductReviewResponse>>();

            try
            {
                var shop = await _shopRepository.GetByIdAsync(request.ShopId);
                if (shop == null)
                {
                    return result.BuildNotFound("Shop not found");
                }

                var (reviews, totalCount) = await _reviewRepository.GetShopReviewsAsync(
                    request.ShopId,
                    request.PageNumber,
                    request.PageSize,
                    request.MinRating,
                    request.HasSellerResponse
                );

                var reviewDtos = _mapper.Map<List<ProductReviewResponse>>(reviews);

                var pagedResponse = new PagedResponse<ProductReviewResponse>
                {
                    Items = reviewDtos,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize
                };

                return result.BuildSuccess(pagedResponse, "Reviews retrieved successfully");
            }
            catch (Exception ex)
            {
                return result.BuildFail($"Failed to retrieve reviews: {ex.Message}");
            }
        }
    }
}
