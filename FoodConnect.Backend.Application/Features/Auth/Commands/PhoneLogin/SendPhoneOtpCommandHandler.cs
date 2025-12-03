using FoodConnect.Backend.Application.Commons.DTOs.Responses;
using FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth;
using FoodConnect.Backend.Application.Commons.Interfaces;
using MediatR;
using System.Numerics;

namespace FoodConnect.Backend.Application.Features.Auth.Commands.PhoneLogin
{
    public class SendPhoneOtpCommandHandler : IRequestHandler<SendPhoneOtpCommand, BaseResponse<OtpSentResponse>>
    {
        private readonly IRedisService _redisService;
        private readonly ISmsService _smsService;

        public SendPhoneOtpCommandHandler(IRedisService redisService, ISmsService smsService)
        {
            _redisService = redisService;
            _smsService = smsService;
        }

        public async Task<BaseResponse<OtpSentResponse>> Handle(SendPhoneOtpCommand request, CancellationToken cancellationToken)
        {
            var result = new BaseResponse<OtpSentResponse>();

            request.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
            if (request.PhoneNumber.StartsWith("0"))
            {
                request.PhoneNumber = "+84" + request.PhoneNumber.Substring(1);
            }
            else if (!request.PhoneNumber.StartsWith("+"))
            {
                request.PhoneNumber = "+" + request.PhoneNumber;
            }

            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            var redisKey = $"otp:phone:{request.PhoneNumber}";
            var expiryMinutes = 2;
            await _redisService.SetAsync(redisKey, otp, TimeSpan.FromMinutes(expiryMinutes));

            await _smsService.SendOtpSmsAsync(request.PhoneNumber, otp);

            var response = new OtpSentResponse
            {
                Destination = MaskPhoneNumber(request.PhoneNumber),
                ExpiresInSeconds = expiryMinutes * 60,
                SentAt = DateTime.UtcNow
            };

            return result.BuildSuccess(response, "OTP has been sent to your phone number");
        }

        private static string MaskPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length <= 4)
                return "***" + phoneNumber;

            var visibleDigits = 4;
            var maskedPart = new string('*', phoneNumber.Length - visibleDigits);
            var visiblePart = phoneNumber.Substring(phoneNumber.Length - visibleDigits);

            return maskedPart + visiblePart;
        }
    }
}
