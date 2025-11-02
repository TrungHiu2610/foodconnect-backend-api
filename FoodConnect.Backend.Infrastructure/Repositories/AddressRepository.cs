using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodConnect.Backend.Infrastructure.Repositories
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext context) : base(context) { }

        public async Task<Address?> GetByIdAsync(Guid id)
        {
            return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Address>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<Address?> GetDefaultAddressByUserIdAsync(Guid userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
        }

        public async Task<bool> HasDefaultAddressAsync(Guid userId)
        {
            return await _context.Addresses
                .AnyAsync(a => a.UserId == userId && a.IsDefault);
        }

        public async Task UnsetAllDefaultAddressesAsync(Guid userId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var address in addresses)
            {
                address.IsDefault = false;
            }
        }
    }
}
