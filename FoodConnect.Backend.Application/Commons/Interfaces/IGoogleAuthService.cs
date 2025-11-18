namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<(bool isValid, string email, string fullName, string? avatarUrl)> VerifyGoogleTokenAsync(string idToken);
    }
}
