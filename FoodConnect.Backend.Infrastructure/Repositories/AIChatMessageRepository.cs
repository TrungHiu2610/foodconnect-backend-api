using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class AIChatMessageRepository : BaseRepository<AIChatMessage>, IAIChatMessageRepository
{
    public AIChatMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<AIChatMessage>> GetConversationHistoryAsync(Guid conversationId, int limit = 20)
    {
        return await _context.AIChatMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(limit)
            .ToListAsync();
    }
}
