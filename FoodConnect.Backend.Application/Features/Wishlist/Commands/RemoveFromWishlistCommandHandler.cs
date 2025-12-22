using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, BaseResponse<DeleteResponse>>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RemoveFromWishlistCommandHandler(
            IWishlistRepository wishlistRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _wishlistRepository = wishlistRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<DeleteResponse>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<DeleteResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            var wishlist = await _wishlistRepository.GetByIdAsync(request.WishlistId);
            if (wishlist == null)
            {
                return result.BuildNotFound("Wishlist item not found");
            }

            if (wishlist.UserId != userId.Value)
            {
                return result.BuildForbidden();
            }

            _wishlistRepository.Remove(wishlist);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return result.BuildSuccess(new DeleteResponse { DeletedCount = 1 }, "Removed from wishlist successfully");
        }
    }
}
