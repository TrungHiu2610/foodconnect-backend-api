using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Services.FirebaseService;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using UserEntity = FoodConnect.Backend.Domain.Entities.User;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.FirebasePhoneLogin
{
    public class FirebasePhoneLoginCommandHandler : IRequestHandler<FirebasePhoneLoginCommand, BaseResponse<AuthResponse>>
    {
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<FirebasePhoneLoginCommandHandler> _logger;

        public FirebasePhoneLoginCommandHandler(
            IFirebaseAuthService firebaseAuthService,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<FirebasePhoneLoginCommandHandler> logger)
        {
            _firebaseAuthService = firebaseAuthService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(FirebasePhoneLoginCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();

            try
            {
                // Step 1: Verify Firebase ID token
                var firebaseToken = await _firebaseAuthService.VerifyIdTokenAsync(request.IdToken);
                
                // Step 2: Get phone number from token
                var phoneNumber = _firebaseAuthService.GetPhoneNumber(firebaseToken);
                
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    // Fallback: Get user record from Firebase
                    var firebaseUser = await _firebaseAuthService.GetUserAsync(firebaseToken.Uid);
                    phoneNumber = firebaseUser.PhoneNumber;
                }

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogError("Phone number not found in Firebase token for UID: {Uid}", firebaseToken.Uid);
                    return result.BuildFail("Phone number not found in authentication token");
                }

                _logger.LogInformation("Firebase phone authentication for: {PhoneNumber}", phoneNumber);

                // Step 3: Check if user exists by phone number
                var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

                if (user == null)
                {
                    // Step 4: Auto-register new user
                    var fullName = !string.IsNullOrEmpty(request.FullName) 
                        ? request.FullName 
                        : $"User {phoneNumber.Substring(phoneNumber.Length - 4)}"; // Use last 4 digits

                    user = new UserEntity
                    {
                        PhoneNumber = phoneNumber,
                        FullName = fullName,
                        Email = null, // Phone auth doesn't require email
                        Provider = AuthProviderEnum.Firebase, // Add this enum if not exists
                        Status = UserStatusEnum.Active,
                        PasswordHash = null,
                        AvatarUrl = null
                    };

                    // Assign default Buyer role
                    user.UserRoles.Add(new Domain.Entities.UserRole
                    {
                        UserId = user.Id,
                        RoleId = RoleEnum.Buyer
                    });

                    await _userRepository.AddAsync(user);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("New user registered via Firebase phone auth: {UserId}", user.Id);
                }
                else
                {
                    // Step 5: Check user status
                    if (user.Status == UserStatusEnum.Banned)
                    {
                        return result.BuildFail("Your account has been banned");
                    }
                    else if (user.Status == UserStatusEnum.Locked)
                    {
                        return result.BuildFail("Your account has been locked");
                    }

                    _logger.LogInformation("Existing user logged in via Firebase phone auth: {UserId}", user.Id);
                }

                // Step 6: Get user roles
                var roleNames = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();

                // Step 7: Check shop ID if user is seller
                Guid? shopId = null;
                if (roleNames.Contains("Seller"))
                {
                    shopId = await _userRepository.GetShopIdByUserIdAsync(user.Id);
                }

                // Step 8: Generate JWT and Refresh Token
                var (accessToken, refreshToken) = await _jwtTokenGenerator.GenerateTokens(user, roleNames, shopId);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Step 9: Build response
                var authResponse = new AuthResponse(
                    user.Id,
                    user.Email ?? "",
                    user.FullName,
                    roleNames,
                    accessToken,
                    refreshToken.Token,
                    refreshToken.ExpiresAtUtc,
                    shopId
                );

                return result.BuildSuccess(authResponse, "Registration successful");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Firebase token verification failed");
                return result.BuildFail("Invalid or expired Firebase token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Firebase phone authentication");
                return result.BuildFail($"Authentication failed: {ex.Message}");
            }
        }
    }
}
