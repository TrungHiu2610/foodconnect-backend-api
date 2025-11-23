using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class SellerWalletRepository : BaseRepository<SellerWallet>, ISellerWalletRepository
{
    public SellerWalletRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SellerWallet?> GetBySellerIdAsync(Guid sellerId)
    {
        return await _context.SellerWallets
            .Include(w => w.Seller)
            .FirstOrDefaultAsync(w => w.SellerId == sellerId);
    }

    public async Task<SellerWallet?> GetWalletWithTransactionsAsync(Guid walletId)
    {
        return await _context.SellerWallets
            .Include(w => w.Seller)
            .Include(w => w.Transactions.OrderByDescending(t => t.CreatedAtUtc))
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount)
    {
        var wallet = await GetByIdAsync(walletId);
        return wallet != null && wallet.Balance >= amount;
    }
}
