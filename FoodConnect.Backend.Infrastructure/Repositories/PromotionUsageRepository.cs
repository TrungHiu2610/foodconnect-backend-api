using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class PromotionUsageRepository : BaseRepository<PromotionUsage>, IPromotionUsageRepository
    {
        public PromotionUsageRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<int> GetUsageCountByPromotionAsync(Guid promotionId)
        {
            return await _context.PromotionUsages
                .CountAsync(pu => pu.PromotionId == promotionId && !pu.IsDeleted);
        }

        public async Task<int> GetUsageCountByUserAsync(Guid promotionId, Guid userId)
        {
            return await _context.PromotionUsages
                .CountAsync(pu => pu.PromotionId == promotionId && pu.UserId == userId && !pu.IsDeleted);
        }

        public async Task<List<PromotionUsage>> GetByPromotionIdAsync(Guid promotionId)
        {
            return await _context.PromotionUsages
                .Include(pu => pu.User)
                .Include(pu => pu.Order)
                .Where(pu => pu.PromotionId == promotionId && !pu.IsDeleted)
                .OrderByDescending(pu => pu.UsedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserUsedPromotionAsync(Guid promotionId, Guid userId)
        {
            return await _context.PromotionUsages
                .AnyAsync(pu => pu.PromotionId == promotionId && pu.UserId == userId && !pu.IsDeleted);
        }
    }
}
