using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface ISellerWalletTransactionRepository : IBaseRepository<SellerWalletTransaction>
{
    Task<List<SellerWalletTransaction>> GetByWalletIdAsync(Guid walletId, int pageNumber, int pageSize, TransactionTypeEnum? type);
    Task<List<SellerWalletTransaction>> GetByOrderIdAsync(Guid orderId);
    Task<SellerWalletTransaction?> GetByWithdrawalRequestIdAsync(Guid withdrawalRequestId);
    Task<int> GetTransactionCountByWalletIdAsync(Guid walletId);
}
