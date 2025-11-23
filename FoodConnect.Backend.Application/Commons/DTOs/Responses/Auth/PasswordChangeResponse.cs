namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth
{
    public class PasswordChangeResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public DateTime? NextAllowedChangeAt { get; set; }
        public bool RequiresRelogin { get; set; } = true;
    }
}
