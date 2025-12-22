using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class AIChatConversationRepository : BaseRepository<AIChatConversation>, IAIChatConversationRepository
{
    public AIChatConversationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AIChatConversation?> GetByUserAndSessionAsync(Guid userId, string sessionId)
    {
        return await _context.AIChatConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.SessionId == sessionId && c.IsActive);
    }

    public async Task<List<AIChatConversation>> GetUserConversationsAsync(Guid userId, int limit = 10)
    {
        return await _context.AIChatConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAtUtc)
            .Take(limit)
            .Include(c => c.Messages)
            .ToListAsync();
    }
}
