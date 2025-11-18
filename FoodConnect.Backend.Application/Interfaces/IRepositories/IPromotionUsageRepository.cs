using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IPromotionUsageRepository : IBaseRepository<PromotionUsage>
    {
        Task<int> GetUsageCountByPromotionAsync(Guid promotionId);
        Task<int> GetUsageCountByUserAsync(Guid promotionId, Guid userId);
        Task<List<PromotionUsage>> GetByPromotionIdAsync(Guid promotionId);
        Task<bool> HasUserUsedPromotionAsync(Guid promotionId, Guid userId);
    }
}
