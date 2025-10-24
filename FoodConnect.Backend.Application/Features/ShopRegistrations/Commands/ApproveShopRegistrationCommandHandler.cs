using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.ShopRegistrations.Commands
{
    public class ApproveShopRegistrationCommandHandler : IRequestHandler<ApproveShopRegistrationCommand, BaseResponse<CreateOrUpdateResponse>>
    {
        private readonly IShopRegistrationRepository _shopRegistrationRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ApproveShopRegistrationCommandHandler(
            IShopRegistrationRepository shopRegistrationRepository,
            IShopRepository shopRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _shopRegistrationRepository = shopRegistrationRepository;
            _shopRepository = shopRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<BaseResponse<CreateOrUpdateResponse>> Handle(ApproveShopRegistrationCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<CreateOrUpdateResponse>();
            var response = new CreateOrUpdateResponse();

            var adminUserId = _currentUserService.UserId;
            if (adminUserId == null)
            {
                return result.BuildFail("Admin user not found");
            }

            // Lấy đơn đăng ký
            var registration = await _shopRegistrationRepository.GetByIdAsync(
                request.ShopRegistrationId,
                r => r.User,
                r => r.Assets);

            if (registration == null)
            {
                return result.BuildFail("Shop registration not found");
            }

            if (registration.Status != ShopRegistrationStatusEnum.Pending)
            {
                return result.BuildFail("Only pending shop registrations can be approved");
            }

            // Kiểm tra user đã có shop chưa
            var existingShop = await _shopRepository.GetByUserIdAsync(registration.UserId);
            if (existingShop != null)
            {
                return result.BuildFail("User already has a shop");
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Cập nhật status registration
                registration.Status = ShopRegistrationStatusEnum.Approved;
                registration.ReviewedBy = adminUserId;
                registration.UpdatedAtUtc = DateTime.UtcNow;
                _shopRegistrationRepository.Update(registration);

                // Tạo Shop mới
                var shop = new Shop
                {
                    Id = Guid.NewGuid(),
                    UserId = registration.UserId,
                    Name = registration.ShopName,
                    Description = registration.ShopDescription,
                    Status = ShopStatusEnum.Active,
                    Street = request.Street ?? string.Empty,
                    Ward = request.Ward ?? string.Empty,
                    District = request.District ?? string.Empty,
                    City = request.City ?? string.Empty,
                    Country = request.Country ?? "Vietnam",
                    Latitude = request.Latitude ?? 0,
                    Longitude = request.Longitude ?? 0
                };

                await _shopRepository.AddAsync(shop);

                // Thêm role Seller cho user
                var user = await _userRepository.GetByIdAsync(registration.UserId, u => u.UserRoles);
                if (user != null)
                {
                    var hasSellerRole = user.UserRoles.Any(ur => ur.RoleId == RoleEnum.Seller);
                    if (!hasSellerRole)
                    {
                        user.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = RoleEnum.Seller
                        });
                        _userRepository.Update(user);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(transaction);

                response.Id = shop.Id;
                response.IsSuccess = true;

                return result.BuildSuccess(response, "Shop registration approved successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return result.BuildFail($"Approve shop registration failed: {ex.Message}");
            }
        }
    }
}
