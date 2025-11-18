using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IPromotionProductRepository : IBaseRepository<PromotionProduct>
    {
        Task<List<PromotionProduct>> GetByPromotionIdAsync(Guid promotionId);
        Task DeleteByPromotionIdAsync(Guid promotionId);
    }
}
