using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetBySessionIdAsync(string sessionId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        public async Task<Cart?> GetCartWithItemsAsync(Guid? userId, string? sessionId)
        {
            var query = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.ProductAssets)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.Shop)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.Category)
                .AsQueryable();

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                return await query.FirstOrDefaultAsync(c => c.UserId == userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                return await query.FirstOrDefaultAsync(c => c.SessionId == sessionId);
            }

            return null;
        }
    }
}
