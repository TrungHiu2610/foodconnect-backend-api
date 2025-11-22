namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;

public class SellerWalletResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal PendingBalance { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
