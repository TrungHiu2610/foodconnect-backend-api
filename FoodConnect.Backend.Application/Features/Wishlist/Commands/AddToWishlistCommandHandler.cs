using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Wishlist.Commands
{
    public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AddToWishlistCommandHandler(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            IShopRepository shopRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _shopRepository = shopRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return result.BuildUnauthorized();
            }

            if (request.ProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(request.ProductId.Value);
                if (product == null)
                {
                    return result.BuildNotFound("Product not found");
                }

                var existing = await _wishlistRepository.GetByUserAndProductAsync(userId.Value, request.ProductId.Value);
                if (existing != null)
                {
                    return result.BuildConflict("Product already in wishlist");
                }

                var wishlist = new Domain.Entities.Wishlist
                {
                    UserId = userId.Value,
                    ProductId = request.ProductId.Value,
                    Type = WishlistTypeEnum.Product,
                    NotificationEnabled = request.NotificationEnabled
                };

                await _wishlistRepository.AddAsync(wishlist);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return result.BuildSuccess(new CreateOrUpdateResponse { Id = wishlist.Id }, "Added to wishlist successfully");
            }
            else if (request.ShopId.HasValue)
            {
                var shop = await _shopRepository.GetByIdAsync(request.ShopId.Value);
                if (shop == null)
                {
                    return result.BuildNotFound("Shop not found");
                }

                var existing = await _wishlistRepository.GetByUserAndShopAsync(userId.Value, request.ShopId.Value);
                if (existing != null)
                {
                    return result.BuildConflict("Shop already in wishlist");
                }

                var wishlist = new Domain.Entities.Wishlist
                {
                    UserId = userId.Value,
                    ShopId = request.ShopId.Value,
                    Type = WishlistTypeEnum.Shop,
                    NotificationEnabled = request.NotificationEnabled
                };

                await _wishlistRepository.AddAsync(wishlist);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return result.BuildSuccess(new CreateOrUpdateResponse { Id = wishlist.Id }, "Added to wishlist successfully");
            }

            return result.BuildFail("Invalid request");
        }
    }
}
