namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface ISmsService
    {
        Task SendOtpSmsAsync(string phoneNumber, string otp);
    }
}
