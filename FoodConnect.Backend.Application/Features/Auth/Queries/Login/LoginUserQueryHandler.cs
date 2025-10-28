using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Exceptions;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Queries.Login
{
    public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, BaseResponse<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginUserQueryHandler(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork,  IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        public async Task<BaseResponse<AuthResponse>> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null)
            {
                return result.BuildFail("Invalid credentials.");
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return result.BuildFail("Invalid credentials.");
            }

            var roleNames = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            (string accessToken, RefreshToken refreshToken) = _jwtTokenGenerator.GenerateTokens(user, roleNames);

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResponse(user.Id, user.Email, user.FullName, roleNames, accessToken, refreshToken.Token, refreshToken.ExpiresAtUtc);
            return result.BuildSuccess(authResult, "Login success");
        }
    }
}
