namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Withdrawal;

public class WithdrawalRequestResponse
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal ProcessingFee { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string PaymentAccountNumber { get; set; } = string.Empty;
    public string PaymentAccountName { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public string? RejectionReason { get; set; }
    public string? SellerNote { get; set; }
    public string? AdminNote { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? IssueImageUrl { get; set; }
}
