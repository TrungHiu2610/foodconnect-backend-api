using FoodConnect.Backend.Application.Commons.DTOs;
using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.EmailRegister
{
    public class RegisterWithEmailCommandHandler : IRequestHandler<RegisterWithEmailCommand, BaseResponse<object>>
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

        public async Task<BaseResponse<object>> Handle(RegisterWithEmailCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<object>();

            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return result.BuildConflict("Email already registered");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Generate 6-digit OTP
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // Store registration data in Redis with 10 minutes TTL
            var redisKey = $"otp:email:{request.Email}";
            var registrationData = new EmailRegistrationData
            {
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Otp = otp
            };

            await _redisService.SetAsync(redisKey, registrationData, TimeSpan.FromMinutes(10));

            // Send OTP email
            await _emailService.SendOtpEmailAsync(request.Email, request.FullName, otp);

            return result.BuildSuccess(new { message = "OTP sent successfully" }, "Please check your email for verification code");
        }
    }
}
