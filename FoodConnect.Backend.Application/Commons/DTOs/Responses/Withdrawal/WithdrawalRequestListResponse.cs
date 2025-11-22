namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;

public class WithdrawalRequestListResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentAccountNumber { get; set; } = string.Empty;
    public string PaymentAccountName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
