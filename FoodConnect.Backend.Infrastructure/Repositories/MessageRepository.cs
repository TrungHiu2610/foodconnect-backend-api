using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Message>> GetByConversationIdAsync(Guid conversationId, int pageNumber, int pageSize)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    public async Task<int> CountByConversationIdAsync(Guid conversationId)
    {
        return await _context.Messages
            .CountAsync(m => m.ConversationId == conversationId);
    }

    public async Task<int> CountUnreadMessagesAsync(Guid conversationId, Guid userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
    }
}
