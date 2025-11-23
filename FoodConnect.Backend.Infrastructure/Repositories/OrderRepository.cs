using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductAssets)
                .Include(o => o.Buyer)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetOrdersByBuyerAsync(Guid buyerId, OrderStatusEnum? status = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p=>p.ProductAssets)
                .Include(o => o.Shop)
                .Where(o => o.BuyerId == buyerId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByShopAsync(Guid shopId, OrderStatusEnum? status = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Buyer)
                .Where(o => o.ShopId == shopId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync();
        }

        public async Task<Order?> GetOrderByCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        public async Task<bool> CanCancelOrderAsync(Guid orderId, Guid buyerId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == buyerId);

            if (order == null)
                return false;

            // Only pending orders can be cancelled
            return order.Status == OrderStatusEnum.Pending;
        }

        public async Task<bool> IsOrderOwnedByBuyerAsync(Guid orderId, Guid buyerId)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.BuyerId == buyerId);
        }

        public async Task<bool> IsOrderOwnedByShopAsync(Guid orderId, Guid shopId)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == orderId && o.ShopId == shopId);
        }

        public async Task<List<Guid>> GetBuyersWithPendingOrdersContainingProductAsync(Guid productId)
        {
            // Return all buyer IDs who have pending orders containing the product
            return await _context.Orders
                .Where(o => o.Status == OrderStatusEnum.Pending && o.OrderItems.Any(oi => oi.ProductId == productId))
                .Select(o => o.BuyerId)
                .Distinct()
                .ToListAsync();
        }
        
        public async Task<int> CountOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAtUtc >= startDate && o.CreatedAtUtc < endDate)
                .CountAsync();
        }
    }
}
