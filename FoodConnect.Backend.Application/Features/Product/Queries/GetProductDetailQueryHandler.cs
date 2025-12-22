using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Product;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Product.Queries
{
    public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, BaseResponse<GetProductDetailResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductDetailQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }
        public async Task<BaseResponse<GetProductDetailResponse>> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<GetProductDetailResponse>();

            if (request.Id == Guid.Empty || request.Id == null)
            {
                return result.BuildFail("Product Id is required");
            }
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                return result.BuildFail("Product not found");
            }
            product.ProductAssets.OrderBy(pa => pa.IsThumbnail);
            var response = _mapper.Map<GetProductDetailResponse>(product);

            return result.BuildSuccess(response, "Get product detail successfully");
        }
    }
}
