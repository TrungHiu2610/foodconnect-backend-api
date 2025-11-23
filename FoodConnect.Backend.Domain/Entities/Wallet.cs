using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public WalletTypeEnum WalletType { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal TotalEarned { get; set; } = 0;
    public decimal TotalWithdrawn { get; set; } = 0;
    public decimal PendingBalance { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0; // For Buyer wallet
    public WalletStatusEnum Status { get; set; } = WalletStatusEnum.Active;
    
    public virtual User User { get; set; } = null!;
    public virtual ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}
