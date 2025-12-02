using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class PromotionRepository : BaseRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Promotion?> GetDetailByIdAsync(Guid id)
        {
            return await _context.Promotions
                .Include(p => p.Shop)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                        .ThenInclude(p => p.ProductAssets)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<List<Promotion>> GetByShopIdAsync(Guid shopId, PromotionStatusEnum? status = null)
        {
            var query = _context.Promotions
                .Include(p => p.PromotionProducts)
                .Where(p => p.ShopId == shopId && !p.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetPendingPromotionsAsync(Guid? shopId = null)
        {
            var query = _context.Promotions
                .Include(p => p.Shop)
                .Include(p => p.PromotionProducts)
                .Where(p => p.Status == PromotionStatusEnum.PendingApproval && !p.IsDeleted);

            if (shopId.HasValue)
            {
                query = query.Where(p => p.ShopId == shopId.Value);
            }

            return await query
                .OrderBy(p => p.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync(PromotionStatusEnum? status = null, Guid? shopId = null)
        {
            var query = _context.Promotions
                .Include(p => p.Shop)
                .Include(p => p.PromotionProducts)
                .Where(p => !p.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (shopId.HasValue)
            {
                query = query.Where(p => p.ShopId == shopId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetActivePromotionsByShopAsync(Guid shopId)
        {
            var now = DateTime.UtcNow;
            return await _context.Promotions
                .Include(p => p.Shop)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                .Where(p => p.ShopId == shopId 
                    && p.Status == PromotionStatusEnum.Active
                    && p.StartDate <= now
                    && p.EndDate >= now
                    && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetPromotionsByStatusAsync(PromotionStatusEnum status)
        {
            return await _context.Promotions
                .Include(p => p.Shop)
                .Where(p => p.Status == status && !p.IsDeleted)
                .OrderBy(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionWithProductsAsync(Guid promotionId)
        {
            return await _context.Promotions
                .Include(p => p.Shop)
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                        .ThenInclude(p => p.ProductAssets)
                .FirstOrDefaultAsync(p => p.Id == promotionId && !p.IsDeleted);
        }
    }
}
