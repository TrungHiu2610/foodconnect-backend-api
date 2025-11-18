using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetPendingPromotionsQueryHandler : IRequestHandler<GetPendingPromotionsQuery, BaseResponse<List<PromotionListResponse>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public GetPendingPromotionsQueryHandler(
            IPromotionRepository promotionRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<PromotionListResponse>>> Handle(GetPendingPromotionsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<PromotionListResponse>>();

            var promotions = await _promotionRepository.GetPendingPromotionsAsync(request.ShopId);
            var response = _mapper.Map<List<PromotionListResponse>>(promotions);

            return result.BuildSuccess(response, "Success");
        }
    }
}
