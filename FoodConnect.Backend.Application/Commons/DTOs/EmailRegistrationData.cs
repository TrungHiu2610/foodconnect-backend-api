namespace FoodConnect.Backend.Application.Commons.DTOs
{
    public class EmailRegistrationData
    {
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Otp { get; set; }
    }
}
