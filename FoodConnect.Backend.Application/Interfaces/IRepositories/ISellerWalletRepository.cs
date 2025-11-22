using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface ISellerWalletRepository : IBaseRepository<SellerWallet>
{
    Task<SellerWallet?> GetBySellerIdAsync(Guid sellerId);
    Task<SellerWallet?> GetWalletWithTransactionsAsync(Guid walletId);
    Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount);
}
