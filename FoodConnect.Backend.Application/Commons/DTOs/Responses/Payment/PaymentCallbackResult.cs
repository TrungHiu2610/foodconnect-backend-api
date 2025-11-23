namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Payment;

public class PaymentCallbackResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
}
