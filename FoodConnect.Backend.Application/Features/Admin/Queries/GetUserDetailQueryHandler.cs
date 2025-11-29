using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, BaseResponse<UserDetailResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IShopRepository _shopRepository;

        public GetUserDetailQueryHandler(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IShopRepository shopRepository)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _shopRepository = shopRepository;
        }

        public async Task<BaseResponse<UserDetailResponse>> Handle(
            GetUserDetailQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UserDetailResponse>();

            var user = await _userRepository.GetAllQueryable()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return result.BuildNotFound("User not found");
            }

            var totalOrders = await _orderRepository.GetAllQueryable()
                .Where(o => o.BuyerId == user.Id)
                .CountAsync(cancellationToken);

            var totalSpending = await _orderRepository.GetAllQueryable()
                .Where(o => o.BuyerId == user.Id && o.Status == Domain.Enums.OrderStatusEnum.Completed)
                .SumAsync(o => (decimal)o.Total, cancellationToken);

            var shop = await _shopRepository.GetAllQueryable()
                .Where(s => s.UserId == user.Id)
                .Select(s => new { s.Id, s.ShopName })
                .FirstOrDefaultAsync(cancellationToken);

            var response = new UserDetailResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Status = user.Status,
                Provider = user.Provider,
                CreatedAtUtc = user.CreatedAtUtc,
                UpdatedAtUtc = user.UpdatedAtUtc,
                TotalOrders = totalOrders,
                TotalSpending = totalSpending,
                ShopId = shop?.Id,
                ShopName = shop?.ShopName
            };

            return result.BuildSuccess(response, "User detail retrieved successfully");
        }
    }
}
