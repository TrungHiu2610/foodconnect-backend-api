using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IMessageRepository : IBaseRepository<Message>
{
    Task<List<Message>> GetByConversationIdAsync(Guid conversationId, int pageNumber, int pageSize);
    Task<int> CountByConversationIdAsync(Guid conversationId);
    Task<int> CountUnreadMessagesAsync(Guid conversationId, Guid userId);
    Task<int> GetUnreadCountByUserAsync(Guid userId);
}
