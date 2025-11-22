using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Domain.Entities;

public class SellerWallet : BaseEntity
{
    public Guid SellerId { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal TotalEarned { get; set; } = 0;
    public decimal TotalWithdrawn { get; set; } = 0;
    public decimal PendingBalance { get; set; } = 0;
    public SellerWalletStatusEnum Status { get; set; } = SellerWalletStatusEnum.Active;
    
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<SellerWalletTransaction> Transactions { get; set; } = new List<SellerWalletTransaction>();
    public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
}
