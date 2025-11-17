namespace FoodConnect.Backend.Application.DTOs.Auth;

public class VerifyFirebasePhoneTokenRequest
{
    public string IdToken { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
public class PhoneAuthResponse
{
    public string UserId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }
    public Guid? ShopId { get; set; }
}
