using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IAIChatMessageRepository : IBaseRepository<AIChatMessage>
{
    Task<List<AIChatMessage>> GetConversationHistoryAsync(Guid conversationId, int limit = 20);
}
