using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;
using UserEntity = FoodConnect.Backend.Domain.Entities.User;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, BaseResponse<AuthResponse>>
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public GoogleLoginCommandHandler(
            IGoogleAuthService googleAuthService,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _googleAuthService = googleAuthService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();

            var (isValid, email, fullName, avatarUrl) = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);

            if (!isValid)
            {
                return result.BuildFail("Invalid Google token");
            }

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                user = new UserEntity
                {
                    Email = email,
                    FullName = fullName,
                    AvatarUrl = avatarUrl,
                    Provider = AuthProviderEnum.Google,
                    Status = UserStatusEnum.Active,
                    PhoneNumber = null,
                    PasswordHash = null
                };

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

            return result.BuildSuccess(authResponse, "Login successful");
        }
    }
}
