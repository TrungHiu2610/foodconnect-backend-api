using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Promotion;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Promotion.Queries
{
    public class GetPromotionDetailQueryHandler : IRequestHandler<GetPromotionDetailQuery, BaseResponse<PromotionResponse>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetPromotionDetailQueryHandler(
            IPromotionRepository promotionRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<PromotionResponse>> Handle(GetPromotionDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PromotionResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var promotion = await _promotionRepository.GetDetailByIdAsync(request.PromotionId);
            if (promotion == null)
            {
                return result.BuildNotFound("Promotion not found");
            }

            if (_currentUserService.Role != "Admin" && promotion.Shop.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            var response = _mapper.Map<PromotionResponse>(promotion);

            return result.BuildSuccess(response, "Success");
        }
    }
}
