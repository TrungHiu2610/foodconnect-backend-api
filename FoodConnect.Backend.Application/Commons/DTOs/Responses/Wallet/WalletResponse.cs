namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;

public class WalletResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int WalletType { get; set; }
    public string WalletTypeName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal TotalSpent { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
