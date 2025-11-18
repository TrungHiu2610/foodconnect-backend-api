using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetActivePromotionsByShopQueryHandler : IRequestHandler<GetActivePromotionsByShopQuery, BaseResponse<List<PromotionListResponse>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public GetActivePromotionsByShopQueryHandler(
            IPromotionRepository promotionRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<PromotionListResponse>>> Handle(GetActivePromotionsByShopQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<PromotionListResponse>>();

            var promotions = await _promotionRepository.GetActivePromotionsByShopAsync(request.ShopId);
            var response = _mapper.Map<List<PromotionListResponse>>(promotions);

            return result.BuildSuccess(response, "Success");
        }
    }
}
