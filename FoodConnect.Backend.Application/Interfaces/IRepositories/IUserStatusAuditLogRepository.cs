using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IUserStatusAuditLogRepository : IBaseRepository<UserStatusAuditLog>
    {
        Task<IEnumerable<UserStatusAuditLog>> GetAuditLogsByUserIdAsync(Guid userId);
    }
}
