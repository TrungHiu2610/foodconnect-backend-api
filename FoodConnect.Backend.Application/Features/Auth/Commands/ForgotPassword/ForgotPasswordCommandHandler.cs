using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, BaseResponse<object>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRedisService _redisService;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IRedisService redisService,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _redisService = redisService;
            _emailService = emailService;
        }

        public async Task<BaseResponse<object>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<object>();

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Security: Don't reveal if email exists or not
                return result.BuildSuccess(new { message = "If your email is registered, you will receive a password reset code." }, 
                    "Password reset code sent");
            }

            // Check if user uses Local provider (has password)
            if (user.Provider != AuthProviderEnum.Local)
            {
                return result.BuildFail($"This account uses {user.Provider} login. Password reset is not available.");
            }

            // Check user status
            if (user.Status == UserStatusEnum.Banned)
            {
                return result.BuildFail("Your account has been banned");
            }

            if (user.Status == UserStatusEnum.Locked)
            {
                return result.BuildFail("Your account has been locked");
            }

            // Generate 6-digit reset token
            var random = new Random();
            var resetToken = random.Next(100000, 999999).ToString();

            // Store reset token in Redis with 15 minutes TTL
            var redisKey = $"reset:password:{request.Email}";
            await _redisService.SetAsync(redisKey, resetToken, TimeSpan.FromMinutes(15));

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(request.Email, user.FullName, resetToken);

            return result.BuildSuccess(new { message = "Password reset code sent to your email" }, 
                "Password reset code sent successfully");
        }
    }
}
