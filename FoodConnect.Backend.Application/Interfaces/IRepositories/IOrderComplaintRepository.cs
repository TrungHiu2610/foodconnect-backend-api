using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IOrderComplaintRepository : IBaseRepository<OrderComplaint>
    {
        Task<OrderComplaint?> GetByOrderIdAsync(Guid orderId);
        Task<OrderComplaint?> GetComplaintWithDetailsAsync(Guid complaintId);
        Task<List<OrderComplaint>> GetComplaintsByBuyerIdAsync(Guid buyerId, OrderComplaintStatusEnum? status = null);
        Task<List<OrderComplaint>> GetComplaintsBySellerIdAsync(Guid sellerId, OrderComplaintStatusEnum? status = null);
        Task<List<OrderComplaint>> GetPendingSellerComplaintsAsync();
        Task<List<OrderComplaint>> GetPendingAdminComplaintsAsync(OrderComplaintStatusEnum? status = null);
        Task<List<OrderComplaint>> GetComplaintsReadyForAutoEscalationAsync();
    }
}
