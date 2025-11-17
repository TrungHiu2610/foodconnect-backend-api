using FirebaseAdmin.Auth;

namespace FoodConnect.Backend.Application.Services.FirebaseService;

public interface IFirebaseAuthService
{
    Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
    Task<UserRecord> GetUserAsync(string uid);
    string GetPhoneNumber(FirebaseToken token);
}
