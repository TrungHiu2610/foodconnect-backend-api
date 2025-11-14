using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IWishlistRepository : IBaseRepository<Wishlist>
    {
        Task<Wishlist?> GetByUserAndProductAsync(Guid userId, Guid productId);
        Task<Wishlist?> GetByUserAndShopAsync(Guid userId, Guid shopId);
        Task<List<Wishlist>> GetByUserIdAsync(Guid userId, int? type = null);
        Task<List<Guid>> GetUsersWishlistedProductAsync(Guid productId);
        Task<List<Guid>> GetUsersWishlistedShopAsync(Guid shopId);
    }
}
