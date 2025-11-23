using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class SellerWalletTransactionRepository : BaseRepository<SellerWalletTransaction>, ISellerWalletTransactionRepository
{
    public SellerWalletTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<SellerWalletTransaction>> GetByWalletIdAsync(Guid walletId, int pageNumber, int pageSize, TransactionTypeEnum? type)
    {
        var query = _context.SellerWalletTransactions
            .Include(t => t.Order)
            .Include(t => t.WithdrawalRequest)
            .Where(t => t.WalletId == walletId);

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }
        return await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SellerWalletTransaction>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.SellerWalletTransactions
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<SellerWalletTransaction?> GetByWithdrawalRequestIdAsync(Guid withdrawalRequestId)
    {
        return await _context.SellerWalletTransactions
            .FirstOrDefaultAsync(t => t.WithdrawalRequestId == withdrawalRequestId);
    }

    public async Task<int> GetTransactionCountByWalletIdAsync(Guid walletId)
    {
        return await _context.SellerWalletTransactions
            .Where(t => t.WalletId == walletId)
            .CountAsync();
    }
}
