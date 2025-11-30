using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Shop;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Shop.Queries
{
    public class GetAllMyShopsQueryHandler : IRequestHandler<GetAllMyShopsQuery, BaseResponse<List<ShopResponse>>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetAllMyShopsQueryHandler> _logger;

        public GetAllMyShopsQueryHandler(
            IShopRepository shopRepository,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<GetAllMyShopsQueryHandler> logger)
        {
            _shopRepository = shopRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<BaseResponse<List<ShopResponse>>> Handle(GetAllMyShopsQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<ShopResponse>>();
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthorized access attempt to get shop applications");
                    return result.BuildUnauthorized("Please login to view your shop applications");
                }

                var shops = await _shopRepository.GetAllByUserIdAsync((Guid)currentUserId);
                
                var shopResponses = _mapper.Map<List<ShopResponse>>(shops);

                return result.BuildSuccess(
                    shopResponses,
                    "Retrieved all shop applications successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shop applications for user");
                return result.BuildFail("An error occurred while retrieving shop applications");
            }
        }
    }
}
