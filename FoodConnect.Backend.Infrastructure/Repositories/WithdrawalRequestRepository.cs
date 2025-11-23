using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class WithdrawalRequestRepository : BaseRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<WithdrawalRequest?> GetDetailByIdAsync(Guid id)
    {
        return await _context.WithdrawalRequests
            .Include(w => w.SellerWallet)
            .Include(w => w.Wallet)
            .Include(w => w.Seller)
            .Include(w => w.ProcessedByAdmin)
            .Include(w => w.SellerWalletTransactions)
            .Include(w => w.WalletTransactions)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<WithdrawalRequest>> GetBySellerIdAsync(Guid sellerId, int pageNumber, int pageSize)
    {
        return await _context.WithdrawalRequests
            .Where(w => w.SellerId == sellerId)
            .OrderByDescending(w => w.RequestedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<WithdrawalRequest>> GetAllWithFiltersAsync(
        WithdrawalStatusEnum? status,
        Guid? sellerId,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize)
    {
        var query = _context.WithdrawalRequests
            .Include(w => w.Seller)
            .Include(w => w.ProcessedByAdmin)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        if (sellerId.HasValue)
            query = query.Where(w => w.SellerId == sellerId.Value);

        if (fromDate.HasValue)
            query = query.Where(w => w.RequestedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.RequestedAt <= toDate.Value);

        return await query
            .OrderByDescending(w => w.RequestedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByFiltersAsync(
        WithdrawalStatusEnum? status,
        Guid? sellerId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _context.WithdrawalRequests.AsQueryable();

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        if (sellerId.HasValue)
            query = query.Where(w => w.SellerId == sellerId.Value);

        if (fromDate.HasValue)
            query = query.Where(w => w.RequestedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.RequestedAt <= toDate.Value);

        return await query.CountAsync();
    }

    public async Task<bool> HasPendingWithdrawalAsync(Guid sellerId)
    {
        return await _context.WithdrawalRequests
            .AnyAsync(w => w.SellerId == sellerId && w.Status == WithdrawalStatusEnum.Pending);
    }
}
