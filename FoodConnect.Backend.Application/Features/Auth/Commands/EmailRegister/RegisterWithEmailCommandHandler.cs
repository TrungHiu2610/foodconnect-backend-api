using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class RegisterWithEmailCommandHandler : IRequestHandler<RegisterWithEmailCommand, BaseResponse<OtpSentResponse>>
    {
        private readonly IRedisService _redisService;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;

        public RegisterWithEmailCommandHandler(
            IRedisService redisService,
            IEmailService emailService,
            IUserRepository userRepository)
        {
            _redisService = redisService;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<OtpSentResponse>> Handle(RegisterWithEmailCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OtpSentResponse>();

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return result.BuildConflict("Email already registered");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            var redisKey = $"otp:email:{request.Email}";
            var registrationData = new EmailRegistrationData
            {
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Otp = otp
            };

            var expiryMinutes = 10;
            await _redisService.SetAsync(redisKey, registrationData, TimeSpan.FromMinutes(expiryMinutes));

            await _emailService.SendOtpEmailAsync(request.Email, request.FullName, otp);

            var response = new OtpSentResponse
            {
                Destination = MaskEmail(request.Email),
                ExpiresInSeconds = expiryMinutes * 60,
                SentAt = DateTime.UtcNow
            };

            return result.BuildSuccess(response, "Please check your email for verification code");
        }

        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return $"{username[0]}***@{domain}";

            return $"{username[0]}***{username[^1]}@{domain}";
        }
    }
}
