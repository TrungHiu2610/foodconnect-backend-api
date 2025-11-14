namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string fullName, string otp);
    }
}
