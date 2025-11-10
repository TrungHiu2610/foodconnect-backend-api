using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Shop.Commands
{
    public class ApproveShopCommandHandler : IRequestHandler<ApproveShopCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ApproveShopCommandHandler(
            IShopRepository shopRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _shopRepository = shopRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ApproveShopCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();

            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return result.BuildUnauthorized("User not found");
            }

            var shop = await _shopRepository.GetDetailByIdAsync(request.ShopId);
            if (shop == null)
            {
                return result.BuildNotFound("Shop not found");
            }

            // Validate status
            if (shop.Status != ShopStatusEnum.PendingApproval)
            {
                return result.BuildFail($"Shop must be in PendingApproval status. Current status: {shop.Status}");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update shop status
                shop.Status = ShopStatusEnum.Active;
                shop.ReviewedBy = userId;
                shop.ReviewedAt = DateTime.UtcNow;
                
                _shopRepository.Update(shop);

                // Add Seller role to user
                var user = shop.User;
                var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id);
                if (userWithRoles != null && userWithRoles.UserRoles.Any(ur => ur.RoleId == RoleEnum.Seller))
                {
                    user.UserRoles.Add(new Domain.Entities.UserRole
                    {
                        UserId = user.Id,
                        RoleId = RoleEnum.Seller
                    });
                    _userRepository.Update(user);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result.BuildSuccess(new CreateOrUpdateResponse
                {
                    Id = shop.Id,
                    IsSuccess = true
                }, "Shop approved successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result.BuildFail($"Failed to approve shop: {ex.Message}");
            }
        }
    }
}
