namespace FoodConnect.Backend.Domain.Enums
{
    public enum AuthProviderEnum
    {
        Local = 0,      // Email + Password
        Phone = 1,      // Phone + OTP (Backend SMS)
        Google = 2,     // Google OAuth
        Firebase = 3    // Firebase Phone Authentication
    }
}
