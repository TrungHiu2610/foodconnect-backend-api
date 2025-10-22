using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ShopRepository : BaseRepository<Shop>, IShopRepository
    {
        public ShopRepository(AppDbContext context) : base(context)
        {
        }
        public async Task<Shop?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Shops.FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
