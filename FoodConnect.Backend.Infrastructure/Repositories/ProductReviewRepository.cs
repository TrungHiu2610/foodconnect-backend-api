using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ProductReviewRepository : BaseRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(List<ProductReview> Reviews, int TotalCount)> GetShopReviewsAsync(
            Guid shopId,
            int pageNumber,
            int pageSize,
            int? minRating = null,
            bool? hasSellerResponse = null)
        {
            // Build query
            var query = _context.Set<ProductReview>()
                .Include(r => r.Product)
                .Include(r => r.Buyer)
                .Include(r => r.Assets.OrderBy(a => a.DisplayOrder))
                .Where(r => r.Product.ShopId == shopId);

            // Apply filters
            if (minRating.HasValue)
            {
                query = query.Where(r => r.Rating >= minRating.Value);
            }

            if (hasSellerResponse.HasValue)
            {
                if (hasSellerResponse.Value)
                {
                    query = query.Where(r => !string.IsNullOrEmpty(r.SellerResponse));
                }
                else
                {
                    query = query.Where(r => string.IsNullOrEmpty(r.SellerResponse));
                }
            }

            // Order by latest first
            query = query.OrderByDescending(r => r.CreatedAtUtc);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var reviews = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }
    }
}
