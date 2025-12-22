using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using UserEntity = FoodConnect.Backend.Domain.Entities.User;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, BaseResponse<AuthResponse>>
    {
        private readonly IRedisService _redisService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public VerifyEmailOtpCommandHandler(
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

        public async Task<BaseResponse<AuthResponse>> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();

            var redisKey = $"otp:email:{request.Email}";
            var registrationData = await _redisService.GetAsync<EmailRegistrationData>(redisKey);

            if (registrationData == null)
            {
                return result.BuildFail("OTP expired or not found. Please register again.");
            }

            if (registrationData.Otp != request.Otp)
            {
                return result.BuildFail("Invalid OTP");
            }

            await _redisService.DeleteAsync(redisKey);

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return result.BuildConflict("Email already registered");
            }

            var user = new UserEntity
            {
                Email = request.Email,
                FullName = registrationData.FullName,
                PasswordHash = registrationData.PasswordHash,
                Provider = AuthProviderEnum.Local,
                Status = UserStatusEnum.Active,
                PhoneNumber = null,
                AvatarUrl = null
            };

            user.UserRoles.Add(new Domain.Entities.UserRole
            {
                UserId = user.Id,
                RoleId = RoleEnum.Buyer
            });

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roleNames = new List<string> { "Buyer" };

            var (accessToken, refreshToken) = await _jwtTokenGenerator.GenerateTokens(user, roleNames, null);

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
                null
            );

            return result.BuildSuccess(authResponse, "Registration successful");
        }
    }
}
