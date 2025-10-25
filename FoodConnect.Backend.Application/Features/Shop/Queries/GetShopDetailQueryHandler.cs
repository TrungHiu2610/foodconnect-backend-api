using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetShopDetailQueryHandler : IRequestHandler<GetShopDetailQuery, BaseResponse<ShopResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;

        public GetShopDetailQueryHandler(
            IShopRepository shopRepository,
            IMapper mapper)
        {
            _shopRepository = shopRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<ShopResponse>> Handle(GetShopDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ShopResponse>();

            var shop = await _shopRepository.GetDetailByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            var shopResponse = _mapper.Map<ShopResponse>(shop);

            return result.BuildSuccess(shopResponse, "Get shop detail successfully");
        }
    }
}
