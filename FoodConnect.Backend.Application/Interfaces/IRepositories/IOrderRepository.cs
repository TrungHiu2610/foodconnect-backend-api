using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(Guid orderId);
        Task<List<Order>> GetOrdersByBuyerAsync(Guid buyerId, OrderStatusEnum? status = null);
        Task<List<Order>> GetOrdersByShopAsync(Guid shopId, OrderStatusEnum? status = null);
        Task<Order?> GetOrderByCodeAsync(string orderCode);
        Task<bool> CanCancelOrderAsync(Guid orderId, Guid buyerId);
        Task<bool> IsOrderOwnedByBuyerAsync(Guid orderId, Guid buyerId);
        Task<bool> IsOrderOwnedByShopAsync(Guid orderId, Guid shopId);

        /// <summary>
        /// Returns a list of buyer IDs who have pending orders containing the specified product
        /// </summary>
        Task<List<Guid>> GetBuyersWithPendingOrdersContainingProductAsync(Guid productId);
    }
}
