using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class PromotionProductRepository : BaseRepository<PromotionProduct>, IPromotionProductRepository
    {
        public PromotionProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<PromotionProduct>> GetByPromotionIdAsync(Guid promotionId)
        {
            return await _context.PromotionProducts
                .Include(pp => pp.Product)
                .Where(pp => pp.PromotionId == promotionId && !pp.IsDeleted)
                .ToListAsync();
        }

        public async Task DeleteByPromotionIdAsync(Guid promotionId)
        {
            var items = await _context.PromotionProducts
                .Where(pp => pp.PromotionId == promotionId)
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsDeleted = true;
            }
        }
    }
}
