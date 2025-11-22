using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class SellerWalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? WithdrawalRequestId { get; set; }
    public TransactionTypeEnum TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.Pending;
    public string? Description { get; set; }
    public string? Metadata { get; set; }
    
    public virtual SellerWallet Wallet { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual WithdrawalRequest? WithdrawalRequest { get; set; }
}
