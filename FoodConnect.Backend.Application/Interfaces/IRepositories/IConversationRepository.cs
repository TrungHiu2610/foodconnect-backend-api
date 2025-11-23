using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IConversationRepository : IBaseRepository<Conversation>
{
    Task<Conversation?> GetByBuyerAndSellerAsync(Guid buyerId, Guid sellerId);
    Task<Conversation?> GetWithMessagesAsync(Guid conversationId, int pageNumber, int pageSize);
    Task<List<Conversation>> GetConversationsByUserIdAsync(Guid userId);
}
