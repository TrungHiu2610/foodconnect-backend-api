using Amazon.Runtime;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using Amazon;
using FoodConnect.Backend.Application.Commons.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class AwsSesEmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public AwsSesEmailService(IConfiguration configuration)
        {
            _fromEmail = configuration["AWS:SES:FromEmail"];
            _fromName = configuration["AWS:SES:FromName"];

            var credentials = new BasicAWSCredentials(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"]
            );

            _sesClient = new AmazonSimpleEmailServiceClient(
                credentials,
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
            );
        }
        public async Task SendOtpEmailAsync(string toEmail, string fullName, string otp)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = $"{_fromName} <{_fromEmail}>",
                Destination = new Destination { ToAddresses = new List<string> { toEmail } },
                Message = new Message
                {
                    Subject = new Content("Email Verification Code"),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = $@"
                                <h2>Hello {fullName},</h2>
                                <p>Your verification code is: <strong style='font-size: 24px;'>{otp}</strong></p>
                                <p>This code will expire in 10 minutes.</p>
                                <p>If you didn't request this code, please ignore this email.</p>
                                <br/>
                                <p>Best regards,<br/>FoodConnect Team</p>
                            "
                        }
                    }
                }
            };

            await _sesClient.SendEmailAsync(sendRequest);
        }
    }
}
