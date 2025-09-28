using FoodConnect.Backend.Application.Commons.DTOs;
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
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
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
        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.ExpiredAccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ArgumentException("Access token and refresh token must be provided.");
            }

            var principal = GetPrincipalFromExpiredToken(request.ExpiredAccessToken);
            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (storedRefreshToken is null ||
                storedRefreshToken.UserId != userId ||
                storedRefreshToken.ExpiresAtUtc < DateTime.UtcNow ||
                storedRefreshToken.JwtId != principal.FindFirst(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti).Value)
            {
                throw new BadRequestException("Invalid refresh token.");
            }

            if (storedRefreshToken.IsRevoked)
            {
                await RevokeAllUserTokens(userId, cancellationToken);
                throw new BadRequestException("This refresh token has been revoked.");
            }

            if (storedRefreshToken.IsUsed)
            {
                await RevokeAllUserTokens(userId, cancellationToken);
                throw new BadRequestException("This refresh token has already been used. All sessions have been logged out as a security precaution.");
            }

            storedRefreshToken.IsUsed = true;
            _refreshTokenRepository.Update(storedRefreshToken);

            var user = await _userRepository.GetByIdAsync(userId);
            var role = principal.FindFirst(ClaimTypes.Role).Value;

            var (newAccessToken, newRefreshToken) = _jwtTokenGenerator.GenerateTokens(user, role);

            await _refreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResponseDto(user.Id, user.Email, role, newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAtUtc);
            return authResult;
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
