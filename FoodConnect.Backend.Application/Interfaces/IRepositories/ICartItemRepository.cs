using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface ICartItemRepository : IBaseRepository<CartItem>
    {
        Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId);
        Task<IEnumerable<CartItem>> GetCartItemsByCartIdAsync(Guid cartId);
    }
}
