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
                var firebaseToken = await _firebaseAuthService.VerifyIdTokenAsync(request.IdToken);
                
                var phoneNumber = _firebaseAuthService.GetPhoneNumber(firebaseToken);
                
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    var firebaseUser = await _firebaseAuthService.GetUserAsync(firebaseToken.Uid);
                    phoneNumber = firebaseUser.PhoneNumber;
                }

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    _logger.LogError("Phone number not found in Firebase token for UID: {Uid}", firebaseToken.Uid);
                    return result.BuildFail("Phone number not found in authentication token");
                }

                _logger.LogInformation("Firebase phone authentication for: {PhoneNumber}", phoneNumber);

                var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

                if (user == null)
                {
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

                var roleNames = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();

                Guid? shopId = null;
                if (roleNames.Contains("Seller"))
                {
                    shopId = await _userRepository.GetShopIdByUserIdAsync(user.Id);
                }

                var (accessToken, refreshToken) = await _jwtTokenGenerator.GenerateTokens(user, roleNames, shopId);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

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
