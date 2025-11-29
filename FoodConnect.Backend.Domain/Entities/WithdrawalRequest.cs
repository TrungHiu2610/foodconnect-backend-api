using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class WithdrawalRequest : BaseEntity
{
    public Guid WalletId { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal ProcessingFee { get; set; } = 0;
    public WithdrawalStatusEnum Status { get; set; } = WithdrawalStatusEnum.Pending;
    public PaymentMethodEnum PaymentMethod { get; set; }
    public string PaymentAccountNumber { get; set; } = string.Empty;
    public string PaymentAccountName { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? SellerNote { get; set; }
    public string? AdminNote { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? IssueImageUrl { get; set; }
    
    public virtual Wallet Wallet { get; set; } = null!;
    public virtual User? ProcessedByAdmin { get; set; }
    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
