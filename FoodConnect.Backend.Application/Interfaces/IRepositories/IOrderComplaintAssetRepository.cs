using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IOrderComplaintAssetRepository : IBaseRepository<OrderComplaintAsset>
    {
        Task<List<OrderComplaintAsset>> GetByComplaintIdAsync(Guid complaintId);
        Task<int> CountByComplaintIdAsync(Guid complaintId);
    }
}
