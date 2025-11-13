using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface ICartRepository : IBaseRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart?> GetBySessionIdAsync(string sessionId);
        Task<Cart?> GetCartWithItemsAsync(Guid? userId, string? sessionId);

        /// <summary>
        /// Returns a list of buyer IDs who have the specified product in their cart
        /// </summary>
        Task<List<Guid>> GetBuyersWithProductInCartAsync(Guid productId);
    }
}
