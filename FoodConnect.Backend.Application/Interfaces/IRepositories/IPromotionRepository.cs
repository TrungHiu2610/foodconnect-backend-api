using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IPromotionRepository : IBaseRepository<Promotion>
    {
        Task<Promotion?> GetDetailByIdAsync(Guid id);
        Task<List<Promotion>> GetByShopIdAsync(Guid shopId, PromotionStatusEnum? status = null);
        Task<List<Promotion>> GetPendingPromotionsAsync(Guid? shopId = null);
        Task<List<Promotion>> GetAllPromotionsAsync(PromotionStatusEnum? status = null, Guid? shopId = null);
        Task<List<Promotion>> GetActivePromotionsByShopAsync(Guid shopId);
        Task<List<Promotion>> GetPromotionsByStatusAsync(PromotionStatusEnum status);
        Task<Promotion?> GetPromotionWithProductsAsync(Guid promotionId);
    }
}
