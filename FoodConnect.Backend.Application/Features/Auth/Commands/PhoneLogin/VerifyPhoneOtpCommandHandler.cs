using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using UserEntity = FoodConnect.Backend.Domain.Entities.User;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class VerifyPhoneOtpCommandHandler : IRequestHandler<VerifyPhoneOtpCommand, BaseResponse<AuthResponse>>
    {
        private readonly IRedisService _redisService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public VerifyPhoneOtpCommandHandler(
            IRedisService redisService,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _redisService = redisService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(VerifyPhoneOtpCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();

            // Verify OTP from Redis
            var redisKey = $"otp:phone:{request.PhoneNumber}";
            var storedOtp = await _redisService.GetAsync<string>(redisKey);

            if (storedOtp == null)
            {
                return result.BuildFail("OTP expired or not found");
            }

            if (storedOtp != request.Otp)
            {
                return result.BuildFail("Invalid OTP");
            }

            // Delete OTP after successful verification
            await _redisService.DeleteAsync(redisKey);

            // Check if user exists
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);

            if (user == null)
            {
                // Create new user
                user = new UserEntity
                {
                    PhoneNumber = request.PhoneNumber,
                    FullName = request.PhoneNumber, // Default, user can update later
                    Provider = AuthProviderEnum.Phone,
                    Status = UserStatusEnum.Active,
                    Email = null,
                    PasswordHash = null
                };

                // Assign default Buyer role
                user.UserRoles.Add(new Domain.Entities.UserRole
                {
                    UserId = user.Id,
                    RoleId = RoleEnum.Buyer
                });

                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else if (user.Status == UserStatusEnum.Banned)
            {
                return result.BuildFail("Your account has been banned");
            }
            else if (user.Status == UserStatusEnum.Locked)
            {
                return result.BuildFail("Your account has been locked");
            }

            // Get user roles
            var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Check shop ID if user is seller
            Guid? shopId = null;
            if (roleNames.Contains("Seller"))
            {
                shopId = await _userRepository.GetShopIdByUserIdAsync(user.Id);
            }

            // Generate JWT and Refresh Token
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

            return result.BuildSuccess(authResponse, "Login successful");
        }
    }
}
