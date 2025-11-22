using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories;

public interface ISystemConfigRepository : IBaseRepository<SystemConfig>
{
    Task<SystemConfig?> GetByKeyAsync(string key);
    Task<List<SystemConfig>> GetAllConfigsAsync();
    Task<List<SystemConfig>> GetConfigsByTypeAsync(int type);
    Task<string?> GetConfigValueAsync(string key);
    Task<decimal> GetCommissionRateAsync();
    Task<decimal> GetMinWithdrawAmountAsync();
}
