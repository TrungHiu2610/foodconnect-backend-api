using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, BaseResponse<OtpSentResponse>>
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

        public async Task<BaseResponse<OtpSentResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OtpSentResponse>();

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Return generic success response
                var dummyResponse = new OtpSentResponse
                {
                    Destination = MaskEmail(request.Email),
                    ExpiresInSeconds = 900, // 15 minutes
                    SentAt = DateTime.UtcNow
                };
                return result.BuildSuccess(dummyResponse, "If your email is registered, you will receive a password reset code");
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
            var expiryMinutes = 15;
            await _redisService.SetAsync(redisKey, resetToken, TimeSpan.FromMinutes(expiryMinutes));

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(request.Email, user.FullName, resetToken);

            var response = new OtpSentResponse
            {
                Destination = MaskEmail(request.Email),
                ExpiresInSeconds = expiryMinutes * 60,
                SentAt = DateTime.UtcNow
            };

            return result.BuildSuccess(response, "Password reset code sent successfully");
        }

        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
            {
                return $"{username[0]}***@{domain}";
            }

            var maskedUsername = $"{username[0]}***{username[username.Length - 1]}";
            return $"{maskedUsername}@{domain}";
        }
    }
}
