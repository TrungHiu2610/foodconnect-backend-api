namespace FoodConnect.Backend.Application.Commons.DTOs.Requests.AIChatbot;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
