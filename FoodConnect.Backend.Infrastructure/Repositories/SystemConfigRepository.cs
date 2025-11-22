using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories;

public class SystemConfigRepository : BaseRepository<SystemConfig>, ISystemConfigRepository
{
    public SystemConfigRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.ConfigKey == key);
    }

    public async Task<List<SystemConfig>> GetAllConfigsAsync()
    {
        return await _context.SystemConfigs
            .OrderBy(c => c.Type)
            .ThenBy(c => c.ConfigKey)
            .ToListAsync();
    }

    public async Task<List<SystemConfig>> GetConfigsByTypeAsync(int type)
    {
        return await _context.SystemConfigs
            .Where(c => (int)c.Type == type)
            .OrderBy(c => c.ConfigKey)
            .ToListAsync();
    }

    public async Task<string?> GetConfigValueAsync(string key)
    {
        var config = await GetByKeyAsync(key);
        return config?.ConfigValue;
    }

    public async Task<decimal> GetCommissionRateAsync()
    {
        var config = await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.Type == SystemConfigTypeEnum.CommissionRate && c.IsActive);
        
        if (config != null && decimal.TryParse(config.ConfigValue, out var rate))
            return rate;
        
        return 5m; // Default 5%
    }

    public async Task<decimal> GetMinWithdrawAmountAsync()
    {
        var config = await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.Type == SystemConfigTypeEnum.MinWithdrawalAmount && c.IsActive);
        
        if (config != null && decimal.TryParse(config.ConfigValue, out var amount))
            return amount;
        
        return 100000m; // Default 100,000 VND
    }
}
