using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class PaymentTransactionRepository : BaseRepository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PaymentTransaction?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.PaymentTransactions
            .Include(p => p.Order)
            .Include(p => p.Buyer)
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
    }

    public async Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.PaymentTransactions
            .Include(p => p.Order)
            .Include(p => p.Buyer)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<List<PaymentTransaction>> GetByBuyerIdAsync(Guid buyerId, int pageNumber, int pageSize)
    {
        return await _context.PaymentTransactions
            .Include(p => p.Order)
            .Where(p => p.BuyerId == buyerId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
