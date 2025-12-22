using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Services.FirebaseService;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;
    private readonly FirebaseAuth _firebaseAuth;

    public FirebaseAuthService(ILogger<FirebaseAuthService> logger, IConfiguration configuration)
    {
        _logger = logger;

        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var firebaseJson = Environment.GetEnvironmentVariable("Firebase__ServiceAccount");

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(firebaseJson)
                });

                _logger.LogInformation("Firebase Admin SDK initialized successfully");
            }

            _firebaseAuth = FirebaseAuth.DefaultInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
            throw;
        }
    }
    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new ArgumentException("ID token cannot be null or empty", nameof(idToken));
            }

            var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(idToken);
            
            _logger.LogInformation("Successfully verified Firebase token for UID: {Uid}", decodedToken.Uid);
            
            return decodedToken;
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError(ex, "Firebase token verification failed: {ErrorCode}", ex.AuthErrorCode);
            throw new UnauthorizedAccessException($"Invalid Firebase token: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Firebase token verification");
            throw;
        }
    }
    public async Task<UserRecord> GetUserAsync(string uid)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentException("UID cannot be null or empty", nameof(uid));
            }

            var userRecord = await _firebaseAuth.GetUserAsync(uid);
            
            _logger.LogInformation("Retrieved Firebase user record for UID: {Uid}", uid);
            
            return userRecord;
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogError(ex, "Failed to get Firebase user: {ErrorCode}", ex.AuthErrorCode);
            throw new InvalidOperationException($"Failed to retrieve user from Firebase: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving Firebase user");
            throw;
        }
    }
    public string GetPhoneNumber(FirebaseToken token)
    {
        try
        {
            if (token.Claims.TryGetValue("phone_number", out var phoneNumber))
            {
                return phoneNumber.ToString() ?? string.Empty;
            }

            _logger.LogWarning("Phone number not found in token claims for UID: {Uid}", token.Uid);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting phone number from token");
            return string.Empty;
        }
    }
}
