using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Exceptions;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, BaseResponse<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;
        public RefreshTokenCommandHandler(IUserRepository userRepository,IRefreshTokenRepository refreshTokenRepository, 
            IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator , IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _configuration = configuration;
        }
        public async Task<BaseResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();
            if (string.IsNullOrEmpty(request.ExpiredAccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return result.BuildFail("Access token and refresh token must be provided.");
            }

            var principal = GetPrincipalFromExpiredToken(request.ExpiredAccessToken);
            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (storedRefreshToken is null ||
                storedRefreshToken.UserId != userId ||
                storedRefreshToken.ExpiresAtUtc < DateTime.UtcNow ||
                storedRefreshToken.JwtId != principal.FindFirst(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti).Value)
            {
                return result.BuildFail("Invalid refresh token.");
            }

            if (storedRefreshToken.IsRevoked)
            {
                await RevokeAllUserTokens(userId, cancellationToken);
                return result.BuildFail("This refresh token has been revoked. All sessions have been logged out as a security precaution.");
            }

            if (storedRefreshToken.IsUsed)
            {
                await RevokeAllUserTokens(userId, cancellationToken);
                return result.BuildFail("This refresh token has already been used. All sessions have been logged out as a security precaution.");
            }

            storedRefreshToken.IsUsed = true;
            _refreshTokenRepository.Update(storedRefreshToken);

            var user = await _userRepository.GetByIdAsync(userId);
            var roleNames = principal.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();

            var (newAccessToken, newRefreshToken) = _jwtTokenGenerator.GenerateTokens(user, roleNames);

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResponse(user.Id, user.Email, user.FullName, roleNames, newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAtUtc);
            return result.BuildSuccess(authResult,"Renew token success");
        }

        private async Task RevokeAllUserTokens(Guid userId, CancellationToken cancellationToken)
        {
            var userTokens = await _refreshTokenRepository.GetAllByUserIdAsync(userId);
            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                _refreshTokenRepository.Update(token);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"])),
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
    }
}
