using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IAIChatConversationRepository : IBaseRepository<AIChatConversation>
{
    Task<AIChatConversation?> GetByUserAndSessionAsync(Guid userId, string sessionId);
    Task<List<AIChatConversation>> GetUserConversationsAsync(Guid userId, int limit = 10);
}
