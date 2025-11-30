using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
{
    public WalletRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Wallet?> GetByUserIdAndTypeAsync(Guid userId, WalletTypeEnum walletType)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId && w.WalletType == walletType);
        if (wallet == null)
        {
            // Auto-create wallet if not exists
            wallet = new Domain.Entities.Wallet
            {
                UserId = userId,
                WalletType = walletType,
                Balance = 0,
                TotalEarned = 0,
                TotalWithdrawn = 0,
                PendingBalance = 0,
                TotalSpent = 0,
                Status = WalletStatusEnum.Active
            };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
        }
        return wallet;
    }

    public async Task<Wallet?> GetWalletWithTransactionsAsync(Guid walletId)
    {
        return await _context.Wallets
            .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAtUtc))
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }

    public async Task<List<Wallet>> GetWalletsByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.WalletType)
            .ToListAsync();
    }
}
