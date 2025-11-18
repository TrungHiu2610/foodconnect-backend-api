using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetMyPromotionsQueryHandler : IRequestHandler<GetMyPromotionsQuery, BaseResponse<List<PromotionListResponse>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyPromotionsQueryHandler(
            IPromotionRepository promotionRepository,
            IShopRepository shopRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _shopRepository = shopRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<PromotionListResponse>>> Handle(GetMyPromotionsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<PromotionListResponse>>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var shop = await _shopRepository.GetByUserIdAsync(userId.Value);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            PromotionStatusEnum? status = request.Status.HasValue ? (PromotionStatusEnum)request.Status.Value : null;
            var promotions = await _promotionRepository.GetByShopIdAsync(shop.Id, status);

            var response = _mapper.Map<List<PromotionListResponse>>(promotions);

            return result.BuildSuccess(response, "Success");
        }
    }
}
