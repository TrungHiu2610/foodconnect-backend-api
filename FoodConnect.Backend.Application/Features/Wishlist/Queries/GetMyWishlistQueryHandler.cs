using AutoMapper;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Wishlist;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Queries
{
    public class GetMyWishlistQueryHandler : IRequestHandler<GetMyWishlistQuery, BaseResponse<List<WishlistResponse>>>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyWishlistQueryHandler(
            IWishlistRepository wishlistRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _wishlistRepository = wishlistRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<WishlistResponse>>> Handle(GetMyWishlistQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<List<WishlistResponse>>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var wishlists = await _wishlistRepository.GetByUserIdAsync(userId.Value, request.Type);
            var response = _mapper.Map<List<WishlistResponse>>(wishlists);

            return result.BuildSuccess(response, "Success");
        }
    }
}
