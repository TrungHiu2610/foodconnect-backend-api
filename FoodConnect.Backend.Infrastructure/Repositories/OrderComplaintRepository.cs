using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class OrderComplaintRepository : BaseRepository<OrderComplaint>, IOrderComplaintRepository
    {
        public OrderComplaintRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<OrderComplaint?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderComplaints
                .Include(c => c.Order)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Admin)
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<OrderComplaint?> GetComplaintWithDetailsAsync(Guid complaintId)
        {
            return await _context.OrderComplaints
                .Include(c => c.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(c => c.Order)
                    .ThenInclude(o => o.Shop)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Admin)
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == complaintId);
        }

        public async Task<List<OrderComplaint>> GetComplaintsByBuyerIdAsync(Guid buyerId, OrderComplaintStatusEnum? status = null)
        {
            var query = _context.OrderComplaints
                .Include(c => c.Order)
                    .ThenInclude(o => o.Shop)
                .Include(c => c.Assets)
                .Where(c => c.BuyerId == buyerId);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<OrderComplaint>> GetComplaintsBySellerIdAsync(Guid sellerId, OrderComplaintStatusEnum? status = null)
        {
            var query = _context.OrderComplaints
                .Include(c => c.Order)
                .Include(c => c.Buyer)
                .Include(c => c.Assets)
                .Where(c => c.SellerId == sellerId);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<OrderComplaint>> GetPendingSellerComplaintsAsync()
        {
            return await _context.OrderComplaints
                .Include(c => c.Order)
                .Include(c => c.Buyer)
                .Include(c => c.Assets)
                .Where(c => c.Status == OrderComplaintStatusEnum.PendingSeller)
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<OrderComplaint>> GetPendingAdminComplaintsAsync(OrderComplaintStatusEnum? status = null)
        {
            var query = _context.OrderComplaints
                .Include(c => c.Order)
                    .ThenInclude(o => o.Shop)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Assets)
                .Where(c => c.Status == OrderComplaintStatusEnum.PendingAdmin 
                         || c.Status == OrderComplaintStatusEnum.SellerResponded);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<List<OrderComplaint>> GetComplaintsReadyForAutoEscalationAsync()
        {
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            
            return await _context.OrderComplaints
                .Include(c => c.Order)
                .Include(c => c.Buyer)
                .Where(c => c.Status == OrderComplaintStatusEnum.PendingSeller 
                         && c.CreatedAtUtc <= twoDaysAgo)
                .ToListAsync();
        }
    }
}
