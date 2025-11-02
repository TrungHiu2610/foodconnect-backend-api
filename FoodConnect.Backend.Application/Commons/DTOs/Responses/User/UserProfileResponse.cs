namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
