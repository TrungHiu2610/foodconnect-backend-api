using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class WalletTransactionRepository : BaseRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<WalletTransaction>> GetByWalletIdAsync(Guid walletId, int pageNumber, int pageSize, TransactionTypeEnum? type)
    {
        var query = _context.WalletTransactions
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

    public async Task<List<WalletTransaction>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.WalletTransactions
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<WalletTransaction?> GetByWithdrawalRequestIdAsync(Guid withdrawalRequestId)
    {
        return await _context.WalletTransactions
            .FirstOrDefaultAsync(t => t.WithdrawalRequestId == withdrawalRequestId);
    }

    public async Task<int> CountByWalletIdAsync(Guid walletId, TransactionTypeEnum? type)
    {
        var query = _context.WalletTransactions
            .Where(t => t.WalletId == walletId);

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        return await query.CountAsync();
    }
}
