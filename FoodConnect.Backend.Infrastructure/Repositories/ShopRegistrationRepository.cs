using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class ShopRegistrationRepository : BaseRepository<ShopRegistration>, IShopRegistrationRepository
    {
        public ShopRegistrationRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ShopRegistration?> GetPendingOrApprovedByUserIdAsync(Guid userId)
        {
            return await _context.ShopRegistrations
                .FirstOrDefaultAsync(sr => sr.UserId == userId && 
                    (sr.Status == ShopRegistrationStatusEnum.Pending || 
                     sr.Status == ShopRegistrationStatusEnum.Approved));
        }

        public async Task<(IEnumerable<ShopRegistration> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            ShopRegistrationStatusEnum? status = null,
            string? searchTerm = null)
        {
            var query = _context.ShopRegistrations
                .Include(sr => sr.User)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(sr => sr.Status == status.Value);
            }

            // Search by shop name or user email/name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(sr => 
                    sr.ShopName.ToLower().Contains(searchTerm) ||
                    sr.User.Email.ToLower().Contains(searchTerm) ||
                    sr.User.FullName.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(sr => sr.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<ShopRegistration?> GetDetailByIdAsync(Guid id)
        {
            return await _context.ShopRegistrations
                .Include(sr => sr.User)
                .Include(sr => sr.Assets)
                .Include(sr => sr.ShopRegistrationCategories)
                    .ThenInclude(src => src.Category)
                .Include(sr => sr.OperatingHours)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }
    }
}
