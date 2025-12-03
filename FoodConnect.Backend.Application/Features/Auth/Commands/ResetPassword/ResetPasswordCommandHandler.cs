using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, BaseResponse<PasswordResetResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _redisService;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IRedisService redisService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _redisService = redisService;
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<PasswordResetResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<PasswordResetResponse>();

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return result.BuildFail("Invalid email or reset token");
            }

            if (user.Provider != AuthProviderEnum.Local)
            {
                return result.BuildFail($"This account uses {user.Provider} login. Password reset is not available.");
            }

            if (user.Status == UserStatusEnum.Banned)
            {
                return result.BuildFail("Your account has been banned");
            }

            if (user.Status == UserStatusEnum.Locked)
            {
                return result.BuildFail("Your account has been locked");
            }

            var redisKey = $"reset:password:{request.Email}";
            var storedToken = await _redisService.GetAsync<string>(redisKey);

            if (storedToken == null)
            {
                return result.BuildFail("Reset token has expired or is invalid");
            }

            if (storedToken != request.ResetToken)
            {
                return result.BuildFail("Invalid reset token");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.PasswordHash = passwordHash;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _redisService.DeleteAsync(redisKey);

            var response = new PasswordResetResponse
            {
                Email = user.Email,
                ResetAt = DateTime.UtcNow,
                RequiresRelogin = true
            };

            return result.BuildSuccess(response, "Password reset successfully");
        }
    }
}
