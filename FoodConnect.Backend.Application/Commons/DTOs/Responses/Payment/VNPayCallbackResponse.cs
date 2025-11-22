namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;

public class VNPayCallbackResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> RawData { get; set; } = new();
}
