using FoodConnect.Backend.Application.Commons.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public TwilioSmsService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromPhoneNumber = configuration["Twilio:PhoneNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendOtpSmsAsync(string phoneNumber, string otp)
        {
            var message = await MessageResource.CreateAsync(
                body: $"Your FoodConnect verification code is: {otp}. Valid for 2 minutes.",
                from: new PhoneNumber(_fromPhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );
        }
    }
}
