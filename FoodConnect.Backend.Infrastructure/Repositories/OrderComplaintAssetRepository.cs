using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class OrderComplaintAssetRepository : BaseRepository<OrderComplaintAsset>, IOrderComplaintAssetRepository
    {
        public OrderComplaintAssetRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<OrderComplaintAsset>> GetByComplaintIdAsync(Guid complaintId)
        {
            return await _context.OrderComplaintAssets
                .Where(a => a.OrderComplaintId == complaintId)
                .OrderBy(a => a.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<int> CountByComplaintIdAsync(Guid complaintId)
        {
            return await _context.OrderComplaintAssets
                .CountAsync(a => a.OrderComplaintId == complaintId);
        }
    }
}
