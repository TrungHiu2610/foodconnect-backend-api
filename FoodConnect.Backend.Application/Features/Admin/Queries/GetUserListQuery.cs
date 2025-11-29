using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Admin.Queries
{
    public class GetUserListQuery : IRequest<BaseResponse<UserManagementListResponse>>
    {
        public string? SearchTerm { get; set; }
        public UserStatusEnum? Status { get; set; }
        public RoleEnum? Role { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAtUtc";
        public bool IsDescending { get; set; } = true;
    }
}
