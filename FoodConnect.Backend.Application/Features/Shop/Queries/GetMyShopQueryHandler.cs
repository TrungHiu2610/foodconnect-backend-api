using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetMyShopQueryHandler : IRequestHandler<GetMyShopQuery, BaseResponse<ShopResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyShopQueryHandler(
            IShopRepository shopRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _shopRepository = shopRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<ShopResponse>> Handle(GetMyShopQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<ShopResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized("User not found");
            }

            var shop = await _shopRepository.GetByUserIdAsync((Guid)userId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            var shopDetail = await _shopRepository.GetDetailByIdAsync(shop.Id);
            var response = _mapper.Map<ShopResponse>(shopDetail);

            return result.BuildSuccess(response, "Get shop successfully");
        }
    }
}
