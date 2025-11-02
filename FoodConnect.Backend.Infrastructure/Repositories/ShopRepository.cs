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
            return await _context.Shops.Include(s=>s.User).FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Shop?> GetDetailByIdAsync(Guid id)
        {
            return await _context.Shops
                .Include(s => s.User)
                .Include(s => s.Assets)
                .Include(s => s.ShopCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.OperatingHours)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(IEnumerable<Shop> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Domain.Enums.ShopStatusEnum? status = null,
            string? searchTerm = null)
        {
            var query = _context.Shops
                .Include(s => s.User)
                .AsQueryable();

            query = query.Where(s=>s.Status != Domain.Enums.ShopStatusEnum.Draft); 
            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            // Search by shop name or owner name (from User)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => 
                    s.ShopName.Contains(searchTerm) || 
                    s.User.FullName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(s => s.User)  // Include User for FullName
                .OrderByDescending(s => s.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Guid>> GetAllCategoryIdsForShopAsync(Guid shopId)
        {
            // Get direct categories that shop registered
            var directCategoryIds = await _context.ShopCategories
                .AsNoTracking()
                .Where(sc => sc.ShopId == shopId)
                .Select(sc => sc.CategoryId)
                .ToListAsync();

            if (!directCategoryIds.Any())
            {
                return new List<Guid>();
            }

            // Get all parent category IDs recursively
            var allCategoryIds = new HashSet<Guid>(directCategoryIds);
            var categoriesToCheck = new Queue<Guid>(directCategoryIds);

            while (categoriesToCheck.Any())
            {
                var currentCategoryId = categoriesToCheck.Dequeue();

                // Get all children of current category
                var children = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.ParentId == currentCategoryId)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var childId in children)
                {
                    if (allCategoryIds.Add(childId)) // Add returns false if already exists
                    {
                        categoriesToCheck.Enqueue(childId);
                    }
                }
            }

            return allCategoryIds.ToList();
        public IQueryable<Shop> GetShopsAsQueryable()
        {
            return _context.Shops
                .Include(s => s.ShopCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.OperatingHours)
                .AsQueryable();
        }
        public IQueryable<Shop> GetShopsAsQueryable()
        {
            return _context.Shops
                .Include(s => s.ShopCategories)
                    .ThenInclude(sc => sc.Category)
                .Include(s => s.OperatingHours)
                .AsQueryable();
        }
    }
}
