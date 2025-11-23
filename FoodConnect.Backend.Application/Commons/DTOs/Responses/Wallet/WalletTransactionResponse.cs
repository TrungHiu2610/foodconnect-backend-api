namespace FoodConnect.Backend.Application.Commons.DTOs.Responses.Wallet;

public class WalletTransactionResponse
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderCode { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
