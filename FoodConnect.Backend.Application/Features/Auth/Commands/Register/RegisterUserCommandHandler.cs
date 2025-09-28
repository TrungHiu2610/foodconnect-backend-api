using FoodConnect.Backend.Application.Commons.DTOs;
using MediatR;
using FoodConnect.Backend.Application.Commons.Exceptions;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RegisterUserCommandHandler(IUserRepository userRepository,IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (!await _userRepository.IsEmailUniqueAsync(request.Email))
            {
                throw new BadRequestException("Email already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
            };

            user.UserRoles.Add(new UserRole { RoleId = RoleEnum.Buyer });

            await _userRepository.AddAsync(user);

            (string accessToken, Domain.Entities.RefreshToken refreshToken) = _jwtTokenGenerator.GenerateTokens(user, RoleEnum.Buyer.ToString());

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto(user.Id, user.Email, RoleEnum.Buyer.ToString(), accessToken, refreshToken.Token, refreshToken.ExpiresAtUtc);
        }
    }
}
