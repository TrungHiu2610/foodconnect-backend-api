using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IWalletTransactionRepository : IBaseRepository<WalletTransaction>
{
    Task<List<WalletTransaction>> GetByWalletIdAsync(Guid walletId, int pageNumber, int pageSize, TransactionTypeEnum? type);
    Task<List<WalletTransaction>> GetByOrderIdAsync(Guid orderId);
    Task<WalletTransaction?> GetByWithdrawalRequestIdAsync(Guid withdrawalRequestId);
    Task<int> CountByWalletIdAsync(Guid walletId, TransactionTypeEnum? type);
}
