using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetPromotionDetailForBuyerQueryHandler : IRequestHandler<GetPromotionDetailForBuyerQuery, BaseResponse<PromotionDetailForBuyerResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public GetPromotionDetailForBuyerQueryHandler(
            IPromotionRepository promotionRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PromotionDetailForBuyerResponse>> Handle(GetPromotionDetailForBuyerQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PromotionDetailForBuyerResponse>();

            var promotion = await _promotionRepository.GetPromotionWithProductsAsync(request.PromotionId);
            if (promotion == null)
            {
                return result.BuildNotFound("Promotion not found");
            }

            if (promotion.Status != PromotionStatusEnum.Active)
            {
                return result.BuildFail("Promotion is not active");
            }

            var response = _mapper.Map<PromotionDetailForBuyerResponse>(promotion);
            return result.BuildSuccess(response, "Success");
        }
    }
}
