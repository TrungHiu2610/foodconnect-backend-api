using FoodConnect.Backend.Domain.Entities;

namespace FoodConnect.Backend.Application.Interfaces.IRepositories
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        Task<Address?> GetByIdAsync(Guid id);
        Task<List<Address>> GetByUserIdAsync(Guid userId);
        Task<Address?> GetDefaultAddressByUserIdAsync(Guid userId);
        Task<bool> HasDefaultAddressAsync(Guid userId);
        Task UnsetAllDefaultAddressesAsync(Guid userId);
    }
}
