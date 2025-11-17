using FoodConnect.Backend.Application.Commons.Interfaces;
using Resend;
using FoodConnect.Backend.Application.Commons.Constants;

namespace FoodConnect.Backend.Infrastructure.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;

        public ResendEmailService(IResend resend)
        {
            _resend = resend;
        }
        public async Task SendOtpEmailAsync(string toEmail, string fullName, string otp)
        {
            var emailMessage = new EmailMessage(){
                From = $"{MailConfigConstant.FromName} <{MailConfigConstant.FromEmail}>",
                To = toEmail,
                Subject = "Email Verification Code",
                HtmlBody = $@"
                                <h2>Hello {fullName},</h2>
                                <p>Your verification code is: <strong style='font-size: 24px;'>{otp}</strong></p>
                                <p>This code will expire in 10 minutes.</p>
                                <p>If you didn't request this code, please ignore this email.</p>
                                <br/>
                                <p>Best regards,<br/>FoodConnect Team</p>
                            "
            };

            await _resend.EmailSendAsync(emailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken)
        {
            var emailMessage = new EmailMessage()
            {
                From = $"{MailConfigConstant.FromName} <{MailConfigConstant.FromEmail}>",
                To = toEmail,
                Subject = "Email Verification Code",
                HtmlBody = $@"
                                <h2>Hello {fullName},</h2>
                                <p>You requested to reset your password.</p>
                                <p>Your password reset code is: <strong style='font-size: 24px;'>{resetToken}</strong></p>
                                <p>This code will expire in 15 minutes.</p>
                                <p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>
                                <br/>
                                <p>Best regards,<br/>FoodConnect Team</p>
                            "
            };

            await _resend.EmailSendAsync(emailMessage);
        }
    }
}
