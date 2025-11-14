using FoodConnect.Backend.Application.Commons.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;


namespace FoodConnect.Backend.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly string _googleClientId;

        public GoogleAuthService(IConfiguration configuration)
        {
            _googleClientId = configuration["Google:ClientId"];
        }

        public async Task<(bool isValid, string email, string fullName, string? avatarUrl)> VerifyGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return (true, payload.Email, payload.Name, payload.Picture);
            }
            catch
            {
                return (false, string.Empty, string.Empty, null);
            }
        }
    }
}
