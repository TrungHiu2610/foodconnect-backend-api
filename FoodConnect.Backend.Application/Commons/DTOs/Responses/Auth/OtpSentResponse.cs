namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Auth
{
    public class OtpSentResponse
    {
        public string Destination { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
        public DateTime SentAt { get; set; }
    }
}
