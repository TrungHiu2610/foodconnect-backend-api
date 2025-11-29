using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class ConversationRepository : BaseRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Conversation?> GetByBuyerAndSellerAsync(Guid buyerId, Guid sellerId)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.SellerId == sellerId);
    }

    public async Task<Conversation?> GetWithMessagesAsync(Guid conversationId, int pageNumber, int pageSize)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task<List<Conversation>> GetConversationsByUserIdAsync(Guid userId)
    {
        return await _context.Conversations
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAtUtc).Take(1))
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAtUtc)
            .ToListAsync();
    }
}
