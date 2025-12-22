namespace FoodConnect.Backend.Application.Commons.DTOs.Requests.Payment;

public class VNPayCreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Platform { get; set; } = "web";
}
