using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetAllPromotionsQueryHandler : IRequestHandler<GetAllPromotionsQuery, BaseResponse<List<PromotionListResponse>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public GetAllPromotionsQueryHandler(
            IPromotionRepository promotionRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<PromotionListResponse>>> Handle(GetAllPromotionsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<PromotionListResponse>>();

            PromotionStatusEnum? status = request.Status.HasValue ? (PromotionStatusEnum)request.Status.Value : null;
            var promotions = await _promotionRepository.GetAllPromotionsAsync(status, request.ShopId);
            var response = _mapper.Map<List<PromotionListResponse>>(promotions);

            return result.BuildSuccess(response, "Success");
        }
    }
}
