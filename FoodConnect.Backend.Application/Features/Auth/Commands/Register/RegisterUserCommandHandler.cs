using MediatR;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.Register
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, BaseResponse<AuthResponse>>
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
        public async Task<BaseResponse<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<AuthResponse>();
            
            if (!await _userRepository.IsEmailUniqueAsync(request.Email))
            {
                return result.BuildFail("Email already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = passwordHash,
            };

            var roles = new List<UserRole>()
            {
                new UserRole { RoleId = RoleEnum.Buyer}
            };
            var roleNames = roles.Select(ur => ur.Role.Name).ToList();
            user.UserRoles = roles;

            await _userRepository.AddAsync(user);

            (string accessToken, Domain.Entities.RefreshToken refreshToken) = _jwtTokenGenerator.GenerateTokens(user, roleNames);

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResponse(user.Id, user.Email, roleNames, accessToken, refreshToken.Token, refreshToken.ExpiresAtUtc);
            return result.BuildSuccess(authResult, "Register success");
        }
    }
}
