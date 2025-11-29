using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, BaseResponse<UserManagementListResponse>>
    {
        private readonly IUserRepository _userRepository;

        public GetUserListQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<UserManagementListResponse>> Handle(
            GetUserListQuery request,
            CancellationToken cancellationToken)
        {
            var result = new BaseResponse<UserManagementListResponse>();

            var query = _userRepository.GetAllQueryable()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchTerm) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(u => u.Status == request.Status.Value);
            }

            if (request.Role.HasValue)
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == request.Role.Value));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "fullname" => request.IsDescending
                    ? query.OrderByDescending(u => u.FullName)
                    : query.OrderBy(u => u.FullName),
                "email" => request.IsDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "status" => request.IsDescending
                    ? query.OrderByDescending(u => u.Status)
                    : query.OrderBy(u => u.Status),
                _ => request.IsDescending
                    ? query.OrderByDescending(u => u.CreatedAtUtc)
                    : query.OrderBy(u => u.CreatedAtUtc)
            };

            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserManagementResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AvatarUrl = u.AvatarUrl,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Status = u.Status,
                    Provider = u.Provider,
                    CreatedAtUtc = u.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var response = new UserManagementListResponse
            {
                Users = users,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };

            return result.BuildSuccess(response, "Users retrieved successfully");
        }
    }
}
