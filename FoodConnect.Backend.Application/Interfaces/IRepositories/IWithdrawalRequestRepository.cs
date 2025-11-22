using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface IWithdrawalRequestRepository : IBaseRepository<WithdrawalRequest>
{
    Task<WithdrawalRequest?> GetDetailByIdAsync(Guid id);
    Task<List<WithdrawalRequest>> GetBySellerIdAsync(Guid sellerId, int pageNumber, int pageSize);
    Task<List<WithdrawalRequest>> GetAllWithFiltersAsync(
        WithdrawalStatusEnum? status,
        Guid? sellerId,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNumber,
        int pageSize);
    Task<int> GetCountByFiltersAsync(
        WithdrawalStatusEnum? status,
        Guid? sellerId,
        DateTime? fromDate,
        DateTime? toDate);
    Task<bool> HasPendingWithdrawalAsync(Guid sellerId);
}
