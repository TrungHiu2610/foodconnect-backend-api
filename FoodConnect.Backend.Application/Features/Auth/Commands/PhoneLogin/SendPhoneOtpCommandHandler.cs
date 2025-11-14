using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.Interfaces;
using MediatR;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class SendPhoneOtpCommandHandler : IRequestHandler<SendPhoneOtpCommand, BaseResponse<object>>
    {
        private readonly IRedisService _redisService;
        private readonly ISmsService _smsService;

        public SendPhoneOtpCommandHandler(IRedisService redisService, ISmsService smsService)
        {
            _redisService = redisService;
            _smsService = smsService;
        }

        public async Task<BaseResponse<object>> Handle(SendPhoneOtpCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<object>();

            // Generate 6-digit OTP
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            // Store OTP in Redis with 2 minutes TTL
            var redisKey = $"otp:phone:{request.PhoneNumber}";
            await _redisService.SetAsync(redisKey, otp, TimeSpan.FromMinutes(2));

            // Send OTP via SMS
            await _smsService.SendOtpSmsAsync(request.PhoneNumber, otp);

            return result.BuildSuccess(new { message = "OTP sent successfully" }, "OTP has been sent to your phone number");
        }
    }
}
