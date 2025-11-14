using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class WishlistRepository : BaseRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Wishlist?> GetByUserAndProductAsync(Guid userId, Guid productId)
        {
            return await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId && !w.IsDeleted);
        }

        public async Task<Wishlist?> GetByUserAndShopAsync(Guid userId, Guid shopId)
        {
            return await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ShopId == shopId && !w.IsDeleted);
        }

        public async Task<List<Wishlist>> GetByUserIdAsync(Guid userId, int? type = null)
        {
            var query = _context.Wishlists
                .Include(w => w.Product!)
                    .ThenInclude(p => p.ProductAssets)
                .Include(w => w.Product!)
                    .ThenInclude(p => p.Category)
                .Include(w => w.Shop)
                .Where(w => w.UserId == userId && !w.IsDeleted);

            if (type.HasValue)
            {
                query = query.Where(w => (int)w.Type == type.Value);
            }

            return await query.OrderByDescending(w => w.CreatedAtUtc).ToListAsync();
        }

        public async Task<List<Guid>> GetUsersWishlistedProductAsync(Guid productId)
        {
            return await _context.Wishlists
                .Where(w => w.ProductId == productId && !w.IsDeleted && w.NotificationEnabled)
                .Select(w => w.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Guid>> GetUsersWishlistedShopAsync(Guid shopId)
        {
            return await _context.Wishlists
                .Where(w => w.ShopId == shopId && !w.IsDeleted && w.NotificationEnabled)
                .Select(w => w.UserId)
                .Distinct()
                .ToListAsync();
        }
    }
}
