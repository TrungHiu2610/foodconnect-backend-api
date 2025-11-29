namespace FoodConnect.Backend.Application.Commons.Interfaces
{
    public interface IRedisService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<long> DeleteByPatternAsync(string pattern);
    }
}
