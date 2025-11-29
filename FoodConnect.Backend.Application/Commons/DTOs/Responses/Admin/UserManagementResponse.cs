using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Admin
{
    public class UserManagementResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
        public UserStatusEnum Status { get; set; }
        public AuthProviderEnum Provider { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class UserManagementListResponse
    {
        public List<UserManagementResponse> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserDetailResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public List<string> Roles { get; set; } = new();
        public UserStatusEnum Status { get; set; }
        public AuthProviderEnum Provider { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpending { get; set; }
        public Guid? ShopId { get; set; }
        public string? ShopName { get; set; }
    }

    public class UserStatusChangeResponse
    {
        public Guid UserId { get; set; }
        public UserStatusEnum OldStatus { get; set; }
        public UserStatusEnum NewStatus { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
