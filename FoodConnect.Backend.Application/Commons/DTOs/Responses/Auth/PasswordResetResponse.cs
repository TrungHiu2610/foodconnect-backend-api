namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth
{
    public class PasswordResetResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime ResetAt { get; set; }
        public bool RequiresRelogin { get; set; } = true;
    }
}
