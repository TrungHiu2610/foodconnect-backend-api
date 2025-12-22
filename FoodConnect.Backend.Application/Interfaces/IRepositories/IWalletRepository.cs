using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IWalletRepository : IBaseRepository<Wallet>
{
    Task<Wallet?> GetByUserIdAndTypeAsync(Guid userId, WalletTypeEnum walletType);
    Task<Wallet?> GetWalletWithTransactionsAsync(Guid walletId);
    Task<List<Wallet>> GetWalletsByUserIdAsync(Guid userId);
}
